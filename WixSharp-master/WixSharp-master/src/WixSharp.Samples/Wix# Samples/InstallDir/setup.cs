//css_dir ..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref System.Core.dll;

using System;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;

class Script
{
    static public void Main(string[] args)
    {
        var project = new Project("CustomActionTest",
                         
                         new Dir(@"%ProgramFiles%\CustomActionTest",
                             new File("readme.txt")),

                         new SetPropertyAction("IDIR", "[INSTALLDIR]"),
                         new ManagedAction(@"MyAction"),
                         
                         new Property("IDIR", "empty") ,
                         new Property("Test", "empty"))
        {
            UI = WUI.WixUI_InstallDir,
        };

        Compiler.PreserveTempFiles = true;
        Compiler.BuildMsi(project);
    }
}

public class CustonActions
{
    [CustomAction]
    public static ActionResult MyAction(Session session)
    {
        try
        {
            MessageBox.Show(session["IDIR"], "InstallDir (INSTALLDIR copy)");
            MessageBox.Show(session["INSTALLDIR"], "InstallDir (actual INSTALLDIR)");
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString(), "Error");
        }
        return ActionResult.Success;
    }
}

