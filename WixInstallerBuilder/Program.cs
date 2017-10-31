using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WixSharp;
using WixSharp.CommonTasks;

namespace WixInstallerBuilder
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SendMessageTimeout(IntPtr hWnd, int Msg, int wParam, string lParam, int fuFlags, int uTimeout, int lpdwResult);
        public const int HWND_BROADCAST = 0xffff;
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int SMTO_ABORTIFHUNG = 0x0002;

        static void Main(string[] args)
        {
            try
            {
                WixSharp.File service;

                var project =
                    new Project("DHS Owa Files Dropper",
                        new Dir(@"%ProgramFiles%\DHS\Owa Files Dropper",
                           new DirFiles(@"C:\Users\tash648\Documents\Visual Studio 2017\Projects\OutlookWebAddIn1\OwaAttachmentServer\bin\Debug"),
                           service = new WixSharp.File(@"C:\Users\tash648\Documents\Visual Studio 2017\Projects\OutlookWebAddIn1\OwaAttachmentServer\bin\Debug\OwaAttachmentServer.exe")),
                           new ElevatedManagedAction("InstallService", System.Reflection.Assembly.GetExecutingAssembly().Location, Return.check, When.After, Step.InstallFiles, Condition.NOT_Installed),
                           new ElevatedManagedAction("UnInstallService", System.Reflection.Assembly.GetExecutingAssembly().Location, Return.check, When.Before, Step.RemoveFiles, Condition.BeingRemoved));
                
                project.GUID = new Guid("6fe30b47-2577-43ad-9195-1861ba25889b");
                project.OutFileName = "setup";

                

                project.BuildMsi();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
