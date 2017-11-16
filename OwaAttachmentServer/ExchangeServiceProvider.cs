using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using OwaAttachmentServer.CreateMessage;
using OwaAttachmentServer.Request;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using static OwaAttachmentServer.ExportDirectoryWatcher;

namespace OwaAttachmentServer
{
    public static class ExchangeServiceProvider
    {
        private static ExportDirectoryWatcher _watcher;
        private static object lockObject = new object();

        public static string Url { get; private set; } = "https://webmail.dhsforyou.com";

        public static ExchangeItem Message { get; set; }

        public static string CurrentToken { get; set; }

        public static string CurrentCookie { get; set; }

        public static bool NewMessage { get; set; } = true;

        public static void CreateAttachment(params FileInformation[] files)
        {
            var body = GetCreateAttachmentBody(files);

            var result = EwsRequest<Response.CreateAttachmentJsonResponse>(body, "CreateAttachment");
        }

        private static T EwsRequest<T>(object body, string action)
            where T: IEwsRequest
        {
            var webRequest = GetPrepearedRequest(body, action);

            try
            {
                var response = (HttpWebResponse)webRequest.GetResponse();

                string responseText;

                RefresTokens(response.Headers["Set-Cookie"]);

                using (var streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseText = streamReader.ReadToEnd();

                    var result = JsonConvert.DeserializeObject<T>(responseText);

                    if (result == null)
                    {
                        throw new ServiceResponseException(ServiceError.ErrorInternalServerError);
                    }

                    var errorCode = result.GetErrorCode();


                    if (!string.IsNullOrEmpty(errorCode))
                    {
                        ServiceError error;

                        if (Enum.TryParse(errorCode, out error))
                        {
                            throw new ServiceResponseException(error);
                        }

                        throw new ServiceResponseException(ServiceError.ErrorItemNotFound);
                    }

                    return result;
                }
            }
            catch (WebException exc)
            {
                if (exc.Response != null)
                {
                    var resp = new StreamReader(exc.Response.GetResponseStream()).ReadToEnd();

                    ResetCookie();

                    throw new ServiceResponseException(ServiceError.ErrorAccessDenied);
                }
            }            

            return default(T);
        }

        private static CreateAttachmentJsonRequest GetCreateAttachmentBody(FileInformation[] files)
        {
            var body = new CreateAttachmentJsonRequest()
            {
                __type = "CreateAttachmentJsonRequest:#Exchange",
                Header = new OwaAttachmentServer.Request.Header
                {
                    __type = "JsonRequestHeaders:#Exchange",
                    RequestServerVersion = "Exchange2013",
                    TimeZoneContext = new Timezonecontext
                    {
                        __type = "TimeZoneContext:#Exchange",
                        TimeZoneDefinition = new Timezonedefinition
                        {
                            __type = "TimeZoneDefinitionType:#Exchange",
                            Id = "UTC"
                        }
                    }
                }
            };

            var attachments = files.Select(p => new Request.Attachment
            {
                __type = "FileAttachment:#Exchange",
                Content = Convert.ToBase64String(p.Content),
                ContentType = "application/x-msdownload",
                IsContactPhoto = false,
                IsInline = false,
                Name = p.Name,
                Size = p.Content.Length
            }).ToArray();

            body.Body = new Request.Body
            {
                __type = "CreateAttachmentRequest:#Exchange",
                ParentItemId = new Parentitemid
                {
                    __type = "ItemId:#Exchange",
                    Id = Message.Id,
                    ChangeKey = ExchangeServiceProvider.Message.ChangeKey
                },
                Attachments = attachments,
                RequireImageType = false,
                IncludeContentIdInResponse = false,
                ClientSupportsIrm = true,
                CancellationId = null
            };
            return body;
        }

        private static ExchangeItem CreateMessagePrivate()
        {
            var body = new OwaAttachmentServer.CreateMessage.Request.CreateMessageRequest("change@this.email");

            var item = EwsRequest<CreateMessageResponse>(body, "CreateItem");

            if(item != null)
            {
                return item.Body.ResponseMessages.Items.FirstOrDefault().Items.FirstOrDefault();
            }

            return null;
        }

        private static ExchangeItem GetMessagePrivate(string id, string changeKey)
        {
            var body = new OwaAttachmentServer.GetMessage.GetMessageRequest(id, changeKey, CurrentToken);

            var item = EwsRequest<GetMessage.Response.GetMessageResponse>(body, "GetItem");

            if (item != null)
            {
                return item.Body.ResponseMessages.Items.FirstOrDefault().Items.FirstOrDefault();
            }

            return null;
        }

        private static void UpdateCookies(string cookie)
        {
            if (string.IsNullOrEmpty(cookie))
                return;

            var canaryIndex = cookie.IndexOf("X-OWA-CANARY=");
            var backEndIndex = cookie.IndexOf("X-BackEndCookie=");

            var backEndCookie = cookie.Substring(backEndIndex + 16, 134);
            var token = cookie.Substring(canaryIndex + 13, 76);

            CurrentToken = token;
            CurrentCookie = $"Cookie: {cookie}";
        }

        private static void RefresTokens(string cookie)
        {
            var canaryIndex = cookie.IndexOf("X-OWA-CANARY=");
            var backEndIndex = cookie.IndexOf("X-BackEndCookie=");

            var backEndCookie = cookie.Substring(backEndIndex, 134 + 16);
            var token = cookie.Substring(canaryIndex, 76 + 13);

            var oldCanaryIndex = CurrentCookie.IndexOf("X-OWA-CANARY=");
            var oldBackEndIndex = CurrentCookie.IndexOf("X-BackEndCookie=");

            var oldBackEndCookie = CurrentCookie.Substring(oldBackEndIndex, 134 + 16);
            var oldToken = CurrentCookie.Substring(oldCanaryIndex, 76 + 13);

            CurrentToken = new string(token.Skip(13).ToArray());
            CurrentCookie = CurrentCookie.Replace(oldBackEndCookie, backEndCookie).Replace(oldToken, token);
        }

        private static HttpWebRequest GetPrepearedRequest(object body, string action)
        {
            var bodyString = JsonConvert.SerializeObject(body);
            var byteBody = Encoding.UTF8.GetBytes(bodyString);

            var webRequest = (HttpWebRequest)WebRequest.Create($"https://webmail.dhsforyou.com/owa/service.svc?action={action}&ID=-114&AC=1");

            webRequest.Method = "POST";
            webRequest.ContentLength = byteBody.Length;
            webRequest.ContentType = "application/json; charset=UTF-8";
            webRequest.KeepAlive = true;

            webRequest.Headers.Add($"Action: {action}");
            webRequest.Headers.Add("X-OWA-CANARY", ExchangeServiceProvider.CurrentToken);
            webRequest.Headers.Add(ExchangeServiceProvider.CurrentCookie);

            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(byteBody, 0, byteBody.Length);
            }

            return webRequest;
        }

        public static bool CookieExist()
        {
            lock (lockObject)
            {
                return CurrentCookie != null;
            }
        }

        public static bool ResetCookie()
        {
            if(CurrentCookie == null)
            {
                return true;
            }

            lock (lockObject)
            {
                if(CurrentCookie == null)
                {
                    return true;
                }

                CurrentCookie = null;

                return true;
            }
        }

        public static bool SetCookie(string cookie)
        {
            if(CurrentCookie != null)
            {
                return true;
            }

            lock (lockObject)
            {
                if(CurrentCookie != null)
                {
                    return true;
                }

                UpdateCookies(cookie);

                var appSettings = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetEntryAssembly().Location).AppSettings;
                var exportPath = @"C:\Users\tash648\Desktop\export"; //appSettings.Settings["ExportFolder"].Value;

                _watcher = new ExportDirectoryWatcher(exportPath);

                _watcher.Run();

                return true; 
            }
        }

        public static ExchangeItem CreateMessage()
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
                            try
                            {
                                if (!CookieExist())
                                {
                                    break;
                                }

                                Message = CreateMessagePrivate();
                                NewMessage = true;

                                return Message;
                            }
                            catch (Exception)
                            { }
                        }
                    }
                    catch (ServiceResponseException ex)
                    { }
                }
            }

            return Message;
        }

        public static bool TryBindMessage(long attachmentsLength, ref ExchangeItem emailMessage, out bool error)
        {
            error = false;

            try
            {
                if(Message == null)
                {
                    error = true;

                    return true;
                }

                emailMessage = GetMessagePrivate(Message.Id, Message.ChangeKey);

                if ((emailMessage.Size + attachmentsLength) > 25165824)
                {
                    error = true;
                }
            }
            catch (Exception ex)
            {
                error = true;
                emailMessage = Message;
            }

            return true;
        }

        public static void Logout()
        {
            _watcher?.Dispose();
            _watcher = null;

            ResetCookie();
        }
    }
}
