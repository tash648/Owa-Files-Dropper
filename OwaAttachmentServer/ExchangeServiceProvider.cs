using Microsoft.Exchange.WebServices.Data;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using static OwaAttachmentServer.DraftController;

namespace OwaAttachmentServer
{
    public static class ExchangeServiceProvider
    {
        private static string _login;
        private static ExportDirectoryWatcher _watcher;
        private static object lockObject = new object();

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

        public static string Url { get; private set; } = "https://webmail.dhsforyou.com";

        public static ExchangeService Service { get; private set; }

        public static Item Message { get; set; }

        static ExchangeServiceProvider()
        {
            var appSettings = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetEntryAssembly().Location).AppSettings;
            var installFolder = Environment.CurrentDirectory; //appSettings.Settings["InstallFolder"].Value;

            dataFullPath = Path.Combine(installFolder, programDataFileName);

            var loginModel = GetLoginModel(dataFullPath);

            if (loginModel != null)
            {
                CreateProvider(loginModel.Login, loginModel.Password);
            }
        }

        public static bool CreateProvider(string login, string password)
        {
            try
            {
                Logout();

                var service = new ExchangeService(ExchangeVersion.Exchange2013);

                service.Credentials = new WebCredentials(login?.Trim(), password);

                service.Url = new Uri($"{Url}/EWS/Exchange.asmx");

                service.FindFolders(WellKnownFolderName.Root, new SearchFilter.IsGreaterThan(FolderSchema.TotalCount, 0), new FolderView(5));

                service.TraceEnabled = false;
                service.TraceFlags = TraceFlags.None;
                
                _login = login;

                var appSettings = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetEntryAssembly().Location).AppSettings;
                var exportPath = @"C:\Users\tash648\Desktop\export"; //appSettings.Settings["ExportFolder"].Value;

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

        public static Item CreateMessage()
        {
            if(Message == null)
            {
                lock (lockObject)
                {
                    try
                    {
                        if (Message != null)
                        {
                            return Message;
                        }

                        while (Message == null)
                        {                            
                            var emailMessage = new EmailMessage(Service);

                            emailMessage.Sender = _login;
                            emailMessage.Save();

                            var propertySet = new PropertySet();

                            propertySet.AddRange(new[] { ItemSchema.Id, ItemSchema.Attachments, ItemSchema.IsDraft, ItemSchema.Size });

                            Message = Item.Bind(Service, emailMessage.Id, propertySet);

                            return Message;
                        }
                    }
                    catch (ServiceResponseException ex)
                    {
                        Debug.WriteLine(ex.Response.ErrorCode);
                    }
                }
            }

            return Message;
        }

        public static Item CreateMessageWithoutCheck()
        {
            lock (lockObject)
            {
                var emailMessage = new EmailMessage(Service);

                emailMessage.Sender = _login;
                emailMessage.Save();

                var propertySet = new PropertySet();

                propertySet.AddRange(new[] { ItemSchema.Id, ItemSchema.Attachments, ItemSchema.IsDraft, ItemSchema.Size });

                Message = Item.Bind(Service, emailMessage.Id, propertySet);

                return Message;
            }
        }

        public static bool TryBindMessage(ref Item emailMessage)
        {
            try
            {
                var propertySet = new PropertySet();

                propertySet.AddRange(new[] { ItemSchema.Id, ItemSchema.Attachments, ItemSchema.IsDraft, ItemSchema.Size });

                emailMessage = Item.Bind(Service, Message.Id, propertySet);                
            }
            catch (Exception ex)
            {
                emailMessage = Message;
            }

            return true;
        }

        public static bool MessageExist()
        {
            try
            {
                var propertySet = new PropertySet();

                propertySet.AddRange(new[] { ItemSchema.Id, ItemSchema.Attachments, ItemSchema.IsDraft, ItemSchema.Size });

                Item.Bind(Service, Message.Id, propertySet);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
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
