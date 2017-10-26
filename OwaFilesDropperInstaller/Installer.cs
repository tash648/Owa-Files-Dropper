using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OwaFilesDropperInstaller
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
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

            process.WaitForExit();
        }

        public Installer()
        {
            InitializeComponent();
        }

        public override void Commit(IDictionary savedState)
        {
            var targetPath = Context.Parameters["TargetDir"].Trim().TrimEnd('\\');
            var directoryInfo = new DirectoryInfo(targetPath);
            var msiPath = Path.Combine(targetPath, "OwaFilesDropperSetup.msi");

            var process = Process.Start(msiPath);

            base.Commit(savedState);

            var installerProcess = Process.GetProcessesByName("msiexec").FirstOrDefault(p => p.MainWindowTitle.Contains("Owa Files Dropper"));

            installerProcess.Kill();
        }

        public override void Uninstall(IDictionary savedState)
        {
            StartCommandAdmin("sc stop OwaFilesDropperService");
            StartCommandAdmin("sc delete OwaFilesDropperService");

            var targetPath = Context.Parameters["TargetDir"].Trim().TrimEnd('\\');
            var directoryInfo = new DirectoryInfo(targetPath);
            var msiPath = Path.Combine(targetPath, "OwaFilesDropperSetup.msi");
            var tempPath = Path.GetTempFileName().TrimEnd('.', 't', 'm', 'p') + ".msi";          
            
            File.Move(msiPath, tempPath);

            var process = Process.Start(tempPath);

            base.Uninstall(savedState);

            var installerProcess = Process.GetProcessesByName("msiexec").FirstOrDefault(p => p.MainWindowTitle.Contains("Owa Files Dropper"));

            installerProcess.Kill();
        }
    }
}
