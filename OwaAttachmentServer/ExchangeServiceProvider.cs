using Microsoft.Exchange.WebServices.Data;
using System;
using System.Configuration;

namespace OwaAttachmentServer
{
    public static class ExchangeServiceProvider
    {
        private static string _login;
        private static ExportDirectoryWatcher _watcher;

        public static string Url { get; private set; }

        public static ExchangeService Service { get; private set; }

        public static EmailMessage Message { get; set; }

        public static void SetUrl(string url)
        {
            ExchangeServiceProvider.Url = url;
        }

        

        public static bool CreateProvider(string login, string password)
        {
            try
            {
                Logout();

                var service = new ExchangeService(ExchangeVersion.Exchange2013);

                service.Credentials = new WebCredentials(login?.Trim(), password);

                service.Url = new Uri($"{Url ?? "https://webmail.dhsforyou.com"}/EWS/Exchange.asmx");

                service.FindFolders(WellKnownFolderName.Root, new SearchFilter.IsGreaterThan(FolderSchema.TotalCount, 0), new FolderView(5));

                Service = service;

                _login = login;

                var exportPath = ConfigurationManager.AppSettings["ExportFolder"];

                _watcher = new ExportDirectoryWatcher(exportPath);

                _watcher.Run();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static EmailMessage CreateMessage()
        {
            var emailMessage = new EmailMessage(Service);

            emailMessage.Sender = _login;
            emailMessage.Save();

            Message = emailMessage;

            return Message;
        }

        public static bool MessageExist()
        {
            if(Message == null)
            {
                return false;
            }

            try
            {
                EmailMessage.Bind(Service, Message.Id);
            }
            catch (Exception)
            {
                Message = null;
                return false;
            }

            return true;
        }

        public static bool TryBindMessage(ref EmailMessage emailMessage)
        {
            if(Message == null)
            {
                return false;
            }

            try
            {
                emailMessage = EmailMessage.Bind(Service, Message.Id);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void Logout()
        {
            _watcher?.Dispose();
            _watcher = null;

            Message = null;
            Service = null;
        }
    }
}
