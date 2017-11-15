using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace OwaAttachmentServer
{
    public class ExportDirectoryWatcher : IDisposable
    {
        private class FileInformation
        {
            public string FullName { get; set; }

            public string TempPath { get; set; }

            public string Name { get; set; }

            public byte[] Content { get; set; }
        }

        private bool isDisposed;

        private FileSystemWatcher tempFilesWatcher;

        private Semaphore semaphore = new Semaphore(1, 1);
        private string directoryPath;

        private string GetLockFileName(string id)
        {
            return Path.Combine(Path.GetTempPath(), $"{id}.full");
        }

        private object createLockLockObject = new object();
        private object tempLockObject = new object();
        private object updateLockObject = new object();

        private System.Threading.Timer timer;
        private DateTime lastRead = DateTime.MinValue;
        private Dictionary<string, System.Threading.Tasks.Task> tasks = new Dictionary<string, System.Threading.Tasks.Task>();

        public ExportDirectoryWatcher(string directoryPath)
        {
            this.directoryPath = directoryPath;

            tempFilesWatcher = new FileSystemWatcher();
            tempFilesWatcher.Path = Path.GetTempPath();

            tempFilesWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;

            tempFilesWatcher.Filter = "*.full";
            tempFilesWatcher.Created += TempFilesWatcher_Created;
        }

        private void StartDirectoryWatch()
        {
            timer = new System.Threading.Timer(o =>
            {
                lock (tempLockObject)
                {
                    try
                    {
                        var newFiles = new DirectoryInfo(directoryPath).GetFiles("*").Where(p => !p.Extension.Contains(".tmp")).ToList();

                        if (newFiles.Any())
                        {
                            var infos = newFiles.Select(p => new FileInformation()
                            {
                                FullName = p.FullName,
                                Name = p.Name,
                                Content = File.ReadAllBytes(p.FullName),
                                TempPath = Path.ChangeExtension(p.FullName, $"{p.Extension}.{Guid.NewGuid().ToString().Replace("-", string.Empty)}.tmp")
                            }).ToList();

                            var takeCount = ExchangeServiceProvider.NewMessage ? 1 : infos.Count;

                            infos = infos.Take(takeCount).ToList();

                            while (infos.Sum(p => p.Content.Length) > 25165824)
                            {
                                infos = infos.Skip(1).ToList();
                            }

                            infos.ForEach(file =>
                            {
                                try
                                {
                                    File.Move(file.FullName, file.TempPath);
                                }
                                catch (Exception ex)
                                {
                                    var a = ex.Message;
                                }
                            });

                            ExportFilesOnCreated(infos);
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }, null, 0, 100);
        }

        private void TempFilesWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (File.Exists(e.FullPath))
                {   
                    ExchangeServiceProvider.Message = null;
                    ExchangeServiceProvider.CreateMessage();

                    File.Move(e.FullPath, Path.ChangeExtension(e.FullPath, ".old"));
                }
            }
            catch (ServiceResponseException ex)
            { }
        }

        private string GetOldLockFilePath(string oldId)
        {
            var lockFileName = GetLockFileName(oldId);
            var oldLockFileName = Path.ChangeExtension(lockFileName, ".old");

            return oldLockFileName;
        }

        private void CreateLockFile(string id)
        {
            var lockFileName = GetLockFileName(id);
            var oldLockFileName = Path.ChangeExtension(lockFileName, ".old");

            if (!File.Exists(lockFileName) && !File.Exists(oldLockFileName))
            {
                lock (createLockLockObject)
                {
                    if (File.Exists(lockFileName) || File.Exists(oldLockFileName))
                    {
                        return;
                    }

                    using (var dataFile = File.Open(lockFileName, FileMode.Create))
                    { }
                }
            }

        }

        private bool LockExist()
        {
            var tempFolderFiles = new DirectoryInfo(Path.GetTempPath()).GetFiles("*.full").ToList();
            return tempFolderFiles.Any();
        }

        private string GetMd5Hash(string value)
        {
            var hash = Encoding.ASCII.GetBytes(value);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var hashenc = md5.ComputeHash(hash);
                var result = string.Empty;

                foreach (var b in hashenc)
                {
                    result += b.ToString("x2");
                }

                return result;
            }
        }

        private bool CreateAttachmentRequest(List<FileInformation> files)
        {
            var body = new CreateAttachmentJsonRequest()
            {
                __type = "CreateAttachmentJsonRequest:#Exchange",
                Header = new Header
                {
                    __type = "JsonRequestHeaders:#Exchange",
                    RequestServerVersion = "Exchange2013",
                    TimeZoneContext = new Timezonecontext
                    {
                        __type = "TimeZoneContext:#Exchange",
                        TimeZoneDefinition = new Timezonedefinition
                        {
                            __type = "TimeZoneDefinitionType:#Exchange",
                            Id = "Pacific Standard Time"
                        }
                    }
                }
            };

            var attachments = files.Select(p => new Attachment
            {
                __type = "FileAttachment:#Exchange",
                Content = Convert.ToBase64String(p.Content),
                ContentType = "application/x-msdownload",
                IsContactPhoto = false,
                IsInline = false,
                Name = p.Name,
                Size = p.Content.Length
            }).ToArray();

            body.Body = new Body
            {
                __type = "CreateAttachmentRequest:#Exchange",
                ParentItemId = new Parentitemid
                {
                    __type = "ItemId:#Exchange",
                    Id = ExchangeServiceProvider.Message.Id.UniqueId,
                    ChangeKey = ExchangeServiceProvider.Message.Id.ChangeKey
                },
                Attachments = attachments,
                RequireImageType = false,
                IncludeContentIdInResponse = false,
                ClientSupportsIrm = true,
                CancellationId = null
            };

            var client = new WebClient();

            client.BaseAddress = ExchangeServiceProvider.Url;

            foreach(var item in ExchangeServiceProvider.Service.HttpHeaders)
            {
                client.Headers.Add(item.Key, item.Value);
            }

            client.Headers.Add(HttpRequestHeader.ContentLength, files.Sum(p => p.Content.Length).ToString());
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=UTF-8");
            client.Headers.Add("Action", "CreateAttachment");
            client.Headers.Add("Referer", "https://webmail.dhsforyou.com/owa/");
            client.Headers.Add("Origin: https://webmail.dhsforyou.com");
            client.Headers.Add("Connection: keep-alive");

            var bodyString = JsonConvert.SerializeObject(body);

            try
            {
                client.UploadData("/owa/service.svc?action=CreateAttachment&ID=-114&AC=1", "POST", Encoding.UTF8.GetBytes(bodyString));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Run()
        {
            var tempFolderFiles = new DirectoryInfo(Path.GetTempPath()).GetFiles("*.full").ToList();
            var oldTempFolderFiles = new DirectoryInfo(Path.GetTempPath()).GetFiles("*.old").ToList();

            tempFolderFiles.ForEach(p => File.Delete(p.FullName));
            oldTempFolderFiles.ForEach(p => File.Delete(p.FullName));

            tempFilesWatcher.EnableRaisingEvents = true;

            StartDirectoryWatch();
        }

        private void ExportFilesOnCreated(List<FileInformation> files)
        {
            try
            {
                if (ExchangeServiceProvider.Message == null)
                {
                    ExchangeServiceProvider.CreateMessage();
                }

                var attachmentsSize = files.Sum(p => p.Content.Length);
                
                semaphore.WaitOne();

                System.Threading.Tasks.Task.Run(() =>
                {
                    var attached = false;

                    Item message = null;

                    while (!attached)
                    {
                        try
                        {
                            if (LockExist())
                            {
                                continue;
                            }

                            var error = false;
                            
                            if (ExchangeServiceProvider.TryBindMessage(attachmentsSize, ref message, out error))
                            {
                                try
                                {
                                    if (error)
                                    {
                                        CreateLockFile(GetMd5Hash(message.Id.UniqueId));
                                        continue;
                                    }

                                    files.ForEach(file =>
                                    {
                                        message.Attachments.AddFileAttachment(file.Name, file.Content);
                                    });

                                    message.Update(ConflictResolutionMode.AutoResolve, true);
                                    ExchangeServiceProvider.NewMessage = false;

                                    attached = true;
                                }
                                catch (CreateAttachmentException ex)
                                {
                                    var createAttachmentResponse = ex.ServiceResponses.ToList().FirstOrDefault() as CreateAttachmentResponse;

                                    if (createAttachmentResponse != null && (createAttachmentResponse.ErrorCode == ServiceError.ErrorMessageSizeExceeded || createAttachmentResponse.ErrorCode == ServiceError.ErrorItemNotFound))
                                    {
                                        CreateLockFile(GetMd5Hash(message.Id.UniqueId));
                                    }
                                }
                                catch (ServiceResponseException ex)
                                {
                                    var createResponse = ex.Response as CreateAttachmentResponse;

                                    if (createResponse != null && (createResponse.ErrorCode == ServiceError.ErrorItemNotFound || createResponse.ErrorCode == ServiceError.ErrorMessageSizeExceeded))
                                    {
                                        CreateLockFile(GetMd5Hash(message.Id.UniqueId));
                                    }
                                }
                            }
                        }

                        catch (Exception ex)
                        { }
                    }

                    semaphore.Release();

                    files.ForEach(file =>
                    {
                        try
                        {
                            File.Delete(file.TempPath);
                        }
                        catch (Exception ex)
                        { }
                    });
                });
            }
            catch (Exception)
            { }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                try
                {
                    semaphore?.Dispose();
                    timer?.Dispose();
                }
                catch (Exception)
                { }

                isDisposed = true;
            }
        }
    }
}
