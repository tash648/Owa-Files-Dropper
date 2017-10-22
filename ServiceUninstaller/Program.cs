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

        static void Main(string[] args)
        {
            StartCommand("sc stop OwaFilesDropperService");
            StartCommand("sc delete OwaFilesDropperService");
        }
    }
}
