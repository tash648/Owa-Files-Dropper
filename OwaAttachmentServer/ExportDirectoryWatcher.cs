using Microsoft.Exchange.WebServices.Data;
using System;
using System.Configuration;
using System.IO;

namespace OwaAttachmentServer
{
    public class ExportDirectoryWatcher : IDisposable
    {
        private bool isDisposed;
        private FileSystemWatcher watcher;

        public string DirectoryPath { get; private set; }

        public string ParentId { get; set; }

        public ExportDirectoryWatcher(string directoryPath, string parentId)
        {
            DirectoryPath = directoryPath;
            ParentId = parentId;

            watcher = new FileSystemWatcher();

            watcher.Path = ConfigurationManager.AppSettings["ExportFolder"];

            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Filter = "*.*";
            watcher.Created += new FileSystemEventHandler(OnCreated);
        }

        public void Run()
        {
            watcher.EnableRaisingEvents = true;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                var message = EmailMessage.Bind(ExchangeServiceProvider.Service, ParentId);

                message.Attachments.AddFileAttachment(e.FullPath);

                message.Update(ConflictResolutionMode.AutoResolve);

                ExchangeServiceProvider.Message = message;

                FileSystemWatcherHub.AttachFile();
    
                File.Delete(e.FullPath);
            }
            catch (Exception ex)
            { }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                watcher?.Dispose();
                isDisposed = true;
            }
        }
    }
}
