using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WixSharp;
using WixSharp.CommonTasks;

namespace WinInstallerBuilder.CA
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult InstallService(Session session)
        {
            return session.HandleErrors(() =>
            {
                Tasks.InstallService(session.Property("INSTALLDIR") + "OwaAttachmentServer.exe", true);
                Tasks.StartService("OwaFilesDropperService", false);
            });
        }

        [CustomAction]
        public static ActionResult UnInstallService(Session session)
        {
            return session.HandleErrors(() =>
            {
                Tasks.InstallService(session.Property("INSTALLDIR") + "OwaAttachmentServer.exe", false);
            });
        }
    }
}
