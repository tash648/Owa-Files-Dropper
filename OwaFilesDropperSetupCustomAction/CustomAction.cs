using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace OwaFilesDropperSetupCA
{
    public class CustomActions
    {
        private const string keyInstallDir = "HKEY_CURRENT_USER\\DHS\\OwaFilesDropperInstaller\\InstallDir";
        private const string keyExportFolder = "HKEY_CURRENT_USER\\DHS\\OwaFilesDropperInstaller\\ExportFolder";

        private static void StartCommand(string command, Session session)
        {
            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas",
                    FileName = "cmd.exe",
                    Arguments =  $"elevate cmd.exe /C {command}"
                }
            };
            process.Start();

            process.WaitForExit();

            if (process.HasExited)
            {
                var output = process.StandardOutput.ReadToEnd();

                session.Message(InstallMessage.Info, new Record() { FormatString = output });
            }
        }

        private static void StartCommandAdmin(string command)
        {
            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas",
                    FileName = "cmd.exe",
                    Arguments =  $"elevate cmd.exe /C {command}"
                }
            };

            process.Start();

            var myStreamWriter = process.StandardInput;

            myStreamWriter.WriteLine("4815162342");

            myStreamWriter.Close();

            process.WaitForExit();
        }

        public class WindowWrapper : IWin32Window
        {
            private readonly IntPtr hwnd;
            public IntPtr Handle
            {
                get { return hwnd; }
            }
            public WindowWrapper(IntPtr handle)
            {
                hwnd = handle;
            }
        }

        [CustomAction]
        public static ActionResult InstallService(Session session)
        {
            try
            {
                var record = new Record();
                
                var targetPath = session["INSTALLDIR"];
                var exportPath = session["EXPORTFOLDER"];

                var configPath = Path.Combine(targetPath, "OwaAttachmentServer.exe.config");
                
                record.FormatString = string.Format("Files exist = " + File.Exists(configPath));

                session.Message(InstallMessage.Info, record);

                if (File.Exists(configPath))
                {
                    var configString = string.Join(" ", File.ReadAllLines(configPath)).Replace("~exportFolder", exportPath);

                    File.WriteAllText(configPath, string.Empty);
                    File.WriteAllText(configPath, configString);
                }

                Process.Start("chrome.exe", "https://chrome.google.com/webstore/detail/owa-files-dropper/bhahbhcbkoapdhemlffafjiglbigdodl");

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                var record = new Record();

                record.FormatString = string.Format("Error action = " + ex);
                session.Message(InstallMessage.Info, record);

                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult InstallNetshUrlAcl(Session session)
        {
            StartCommand("netsh http delete urlacl url=http://*:4433/", session);
            StartCommand("netsh http add urlacl url=http://*:4433/ user=EVERYONE", session);

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult SelectExportFolder(Session session)
        {
            var worker = new Thread(() =>
            {
                var installerProcess = Process.GetProcessesByName("msiexec").FirstOrDefault(p => p.MainWindowTitle.Contains("DHS"));

                var dialog = new FolderBrowserDialog();
                dialog.Description = "Select export folder";

                var result = installerProcess != null
                   ? dialog.ShowDialog(new WindowWrapper(installerProcess.MainWindowHandle))
                   : dialog.ShowDialog();

                session["EXPORTFOLDER"] = dialog.SelectedPath;
            });

            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
            worker.Join();

            return ActionResult.Success;
        }
    }
}
