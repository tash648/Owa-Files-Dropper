using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUninstaller
{
    class Program
    {
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

            process.WaitForExit();
        }

        static void Main(string[] args)
        {
            StartCommandAdmin("sc stop OwaFilesDropperService");
            StartCommandAdmin("sc delete OwaFilesDropperService");
        }
    }
}
