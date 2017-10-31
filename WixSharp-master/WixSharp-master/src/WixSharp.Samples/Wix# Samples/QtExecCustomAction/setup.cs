//css_dir ..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref System.Core.dll;
using System;
using WixSharp;

class Script
{
    static public void Main(string[] args)
    {
        var project = new Project()
        {
            UI = WUI.WixUI_ProgressOnly,
            Name = "CustomActionTest",
            Actions = new[] { new QtCmdLineAction("notepad.exe", @"C:\boot.ini") },
        };
		
        Compiler.BuildMsi(project);
    }
}



