using Microsoft.AspNet.SignalR;

namespace OwaAttachmentServer
{
    public class FileSystemWatcherHub : Hub
    {
        public static void AttachFile()
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<FileSystemWatcherHub>();

            hub.Clients.All.attachFile();
        }

        public static void LoginSuccess()
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<FileSystemWatcherHub>();

            hub.Clients.All.loginSuccess();
        }
    }
}
