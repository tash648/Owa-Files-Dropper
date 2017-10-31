using Microsoft.Deployment.WindowsInstaller;
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
        private static void StartCommand(string command)
        {
            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments =  $"/C {command}"
                }
            };
            process.Start();

            process.WaitForExit();

            if (process.HasExited)
            {
                string output = process.StandardOutput.ReadToEnd();
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
                var targetPath = session["INSTALLDIR"];

                var directoryInfo = new DirectoryInfo(targetPath);
                var certPath = Path.Combine(targetPath, "localhost.pfx");

                StartCommandAdmin(string.Format("certutil -importpfx \"{0}\"", certPath));
                StartCommand("netsh http delete sslcert ipport=0.0.0.0:8080");
                StartCommand("netsh http delete urlacl url=https://*:8080/");
                StartCommand("netsh http add urlacl url=https://*:8080/ user=EVERYONE");
                StartCommand("netsh http add sslcert ipport=0.0.0.0:8080 appid={a1859f53-c288-43a1-ad49-40ff8ed84764} certhash=82183dbc3daaaef7a3b9b0d0d7040dad500c640f");

                var path = session["EXPORTFOLDER"];

                var configPath = Path.Combine(targetPath, "OwaAttachmentServer.exe.config");

                var record = new Record();

                record.FormatString = string.Format("Files exist = " + File.Exists(configPath));

                session.Message(InstallMessage.Info, record);

                if (File.Exists(configPath))
                {
                    var configString = string.Join(" ", File.ReadAllLines(configPath)).Replace("~exportFolder", path);

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
