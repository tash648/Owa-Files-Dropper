//css_dir ..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref System.Core.dll;

using System;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;

class Script
{
    static public void Main(string[] args)
    {
        var project =
            new Project("RebootTest",
                new ManagedAction("PromptToReboot"));

        project.UI = WUI.WixUI_ProgressOnly;
        Compiler.BuildMsi(project);
    }
}

public class CustonActions
{
    [CustomAction]
    public static ActionResult PromptToReboot(Session session)
    {

        if (DialogResult.Yes == MessageBox.Show("You need to reboot the system.\nDo you want to reboot now?", "ReboolTest", MessageBoxButtons.YesNo))
        {
            Process.Start("shutdown.exe", "-r -t 30 -c \"Reboot has been requested from RebootTest.msi\"");
        }

        return ActionResult.Success;
    }
}



