using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace OwaAttachmentServer
{
    public class ExportDirectoryWatcher : IDisposable
    {
        public class FileInformation
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
                        if(!ExchangeServiceProvider.CookieExist())
                        {
                            return;
                        }

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
                            
                            while (infos.Sum(p => p.Content.Length) > 25165824)
                            {
                                infos = infos.OrderByDescending(p => p.Content.Length).Skip(1).ToList();
                            }

                            infos.ForEach(file =>
                            {
                                try
                                {
                                    File.Move(file.FullName, file.TempPath);
                                }
                                catch (Exception ex)
                                { }
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

                    while (File.Exists(e.FullPath))
                    {
                        try
                        {
                            File.Move(e.FullPath, Path.ChangeExtension(e.FullPath, ".old"));
                        }
                        catch (Exception)
                        { }
                    }
                }
            }
            catch (Exception ex)
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
                if (files == null || files.Count == 0)
                {
                    return;
                }                

                if (ExchangeServiceProvider.Message == null)
                {
                    ExchangeServiceProvider.CreateMessage();

                    if(ExchangeServiceProvider.Message == null)
                    {
                        files.ForEach(file =>
                        {
                            try
                            {
                                File.Move(file.TempPath, file.FullName);
                            }
                            catch (Exception ex)
                            { }
                        });

                        return;
                    }
                }

                semaphore.WaitOne();

                ExchangeServiceProvider.SetInProgress(true);

                var attachmentsSize = files.Sum(p => p.Content.Length);

                System.Threading.Tasks.Task.Run(() =>
                {
                    var attached = false;

                    ExchangeItem message = null;

                    var filesForDelete = files.ToList();

                    while (!attached)
                    {
                        try
                        {
                            if (LockExist() || !ExchangeServiceProvider.CookieExist())
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
                                        CreateLockFile(GetMd5Hash(message.Id));

                                        continue;
                                    }

                                    var tempFiles = files.ToList();

                                    tempFiles.ForEach(file =>
                                    {
                                        ExchangeServiceProvider.CreateAttachment(file);

                                        files.Remove(file);

                                        try
                                        {
                                            File.Delete(file.TempPath);
                                        }
                                        catch (Exception)
                                        { }
                                    });

                                    attached = true;
                                }
                                catch (ServiceResponseException ex)
                                {
                                    if (ex != null && (ex.ErrorCode == ServiceError.ErrorItemNotFound || ex.ErrorCode == ServiceError.ErrorMessageSizeExceeded))
                                    {
                                        CreateLockFile(GetMd5Hash(message.Id));
                                    }
                                }
                            }
                        }

                        catch (Exception ex)
                        { }
                    }

                    ExchangeServiceProvider.SetInProgress(false);

                    semaphore.Release();
                });
            }
            catch (Exception ex)
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
