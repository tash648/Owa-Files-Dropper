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

        public static void CreateProvider(string login, string password)
        {
            Logout();

            Service = new ExchangeService(ExchangeVersion.Exchange2013);
            Service.Credentials = new WebCredentials(login, password);
            Service.Url = new Uri($"{Url}/EWS/Exchange.asmx");

            Service.GetAppManifests();

            _login = login;
        }

        public static void CreateMessage()
        {
            var emailMessage = new EmailMessage(Service);

            emailMessage.Sender = _login;
            emailMessage.Save();

            Message = emailMessage;

            var exportPath = ConfigurationManager.AppSettings["ExportFolder"];

            _watcher = new ExportDirectoryWatcher(exportPath, ExchangeServiceProvider.Message.Id.UniqueId);

            _watcher.Run();
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
