using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace OwaAttachmentServer
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
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

        private void InstallCertificate(string filePath)
        {
            var collection = new X509Certificate2Collection();

            collection.Import(filePath, "4815162342", X509KeyStorageFlags.PersistKeySet);

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            foreach (var certificate in collection)
            {
                store.Add(certificate);
            }

            store.Close();
        }

        private void StartCommand(string command)
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

        private void StartCommandAdmin(string command)
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

        public ProjectInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);            
        }

        public override void Commit(IDictionary savedState)
        {
            var targetPath = Context.Parameters["TargetDir"].Trim().TrimEnd('\\');
            var directoryInfo = new DirectoryInfo(targetPath);

            var certPath = Path.Combine(targetPath, "localhost.pfx");
            
            StartCommandAdmin(string.Format("certutil -importpfx \"{0}\"", certPath));
            StartCommand("netsh http delete sslcert ipport=0.0.0.0:8080");
            StartCommand("netsh http delete urlacl url=https://*:8080/");
            StartCommand("netsh http add urlacl url=https://*:8080/ user=EVERYONE");
            StartCommand("netsh http add sslcert ipport=0.0.0.0:8080 appid={a1859f53-c288-43a1-ad49-40ff8ed84764} certhash=82183dbc3daaaef7a3b9b0d0d7040dad500c640f");

            var worker = new Thread(() =>
            {
                var installerProcess = Process.GetProcessesByName("msiexec").FirstOrDefault(p => p.MainWindowTitle.Contains("DHS"));
                
                var dialog = new FolderBrowserDialog();                
                dialog.Description = "Select export folder";

                var result = installerProcess != null
                   ? dialog.ShowDialog(new WindowWrapper(installerProcess.MainWindowHandle))
                   : dialog.ShowDialog();

                var path = dialog.SelectedPath;

                var configPath = Path.Combine(targetPath, "OwaAttachmentServer.exe.config");

                var configString = string.Join(" ", File.ReadAllLines(configPath)).Replace("~exportFolder", path);

                File.WriteAllText(configPath, string.Empty);
                File.WriteAllText(configPath, configString);
            });

            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
            worker.Join();

            using (var service = new System.ServiceProcess.ServiceController(serviceInstaller1.ServiceName))
            {
                service.Start();
            }

            base.Commit(savedState);
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            var controller = new ServiceController("OwaFilesDropperService");
            try
            {
                if (controller.Status == ServiceControllerStatus.Running | controller.Status == ServiceControllerStatus.Paused)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 0, 15));
                    controller.Close();
                }
            }
            catch (Exception ex)
            {
                string source = "OwaFilesDropperService";
                string log = "Application";
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, log);
                }
                EventLog eLog = new EventLog();
                eLog.Source = source;
                eLog.WriteEntry(string.Concat(@"The service could not be stopped. Please stop the service manually. Error: ", ex.Message), EventLogEntryType.Error);
            }
            finally
            {
                base.Uninstall(savedState);
            }
        }
    }
}
