using Microsoft.Exchange.WebServices.Data;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using static OwaAttachmentServer.DraftController;

namespace OwaAttachmentServer
{
    public static class ExchangeServiceProvider
    {
        private static string _login;
        private static ExportDirectoryWatcher _watcher;

        private static string programDataFileName = "Permissions.dll";

        private static string dataFullPath;

        public class DataModel
        {
            public string Login { get; set; }

            public string Password { get; set; }

            public string Host { get; set; }
        }

        private static void CleanLoginModel(string fileName)
        {
            try
            {
                using (var dataFile = File.Open(fileName, FileMode.Truncate))
                { }
            }
            catch (Exception)
            { }
        }

        private static bool SaveLoginModel(string fileName, DataModel data)
        {
            try
            {
                using (var dataFile = File.Open(fileName, FileMode.Truncate))
                using (var streamWriter = new StreamWriter(dataFile))
                {
                    var json = JsonConvert.SerializeObject(data);

                    streamWriter.Write(json);

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }            
        }

        public static DataModel GetLoginModel(string fileName)
        {
            try
            {
                using (var dataFile = File.Open(fileName, FileMode.OpenOrCreate))
                using (var streamReader = new StreamReader(dataFile))
                {
                    var json = streamReader.ReadToEnd();

                    var dataModel = JsonConvert.DeserializeObject<DataModel>(json);

                    return dataModel;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Url { get; private set; }

        public static ExchangeService Service { get; private set; }

        public static EmailMessage Message { get; set; }

        static ExchangeServiceProvider()
        {
            var appSettings = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetEntryAssembly().Location).AppSettings;
            var installFolder = appSettings.Settings["InstallFolder"].Value;

            dataFullPath = Path.Combine(installFolder, programDataFileName);

            var loginModel = GetLoginModel(dataFullPath);

            if (loginModel != null)
            {
                SetUrl(loginModel.Host);
                CreateProvider(loginModel.Login, loginModel.Password);
            }
        }

        public static void SetUrl(string url)
        {
            Url = url;
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

                service.TraceEnabled = false;
                service.TraceFlags = TraceFlags.None;
                
                _login = login;

                var appSettings = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetEntryAssembly().Location).AppSettings;
                var exportPath = appSettings.Settings["ExportFolder"].Value;

                _watcher = new ExportDirectoryWatcher(exportPath);

                _watcher.Run();

                Service = service;

                SaveLoginModel(dataFullPath, new DataModel()
                {
                    Host = Url,
                    Login = login?.Trim(),
                    Password = password
                });

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
            CleanLoginModel(dataFullPath);

            _watcher?.Dispose();
            _watcher = null;

            Message = null;
            Service = null;
        }
    }
}
