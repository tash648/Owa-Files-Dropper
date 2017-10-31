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

        public ExportDirectoryWatcher(string directoryPath)
        {
            watcher = new FileSystemWatcher();

            watcher.Path = directoryPath;

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
                EmailMessage message = null;

                if (!ExchangeServiceProvider.TryBindMessage(ref message))
                {
                    message = ExchangeServiceProvider.CreateMessage();
                }

                message.Attachments.AddFileAttachment(e.FullPath);

                message.Update(ConflictResolutionMode.AutoResolve);

                ExchangeServiceProvider.Message = message;
    
                File.Delete(e.FullPath);
            }
            catch (Exception)
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
