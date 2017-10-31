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
        //Note that if the install condition for the component can be set without interacting with user (e.g. analyzing registry)
        //as part InstallExecuteSequence. However if interaction is required (e.g. message box, checkbox) install condition should 
        //be set form InstallUISequence.

        var project =
                new Project("My Product",

                    //Files and Shortcuts
                    new Dir(new Id("INSTALL_DIR"), @"%ProgramFiles%\My Company\My Product",
                        new File(@"AppFiles\MyApp.exe",
                            new FileShortcut("MyApp", @"%ProgramMenu%\My Company\My Product")
                                             {
                                                 WorkingDirectory = @"%ProgramMenu%\My Company"
                                             }),
                        new ExeFileShortcut("Uninstall MyApp", "[System64Folder]msiexec.exe", "/x [ProductCode]")),

                    new Dir(@"%ProgramMenu%\My Company\My Product",
                        new ExeFileShortcut("Uninstall MyApp", "[System64Folder]msiexec.exe", "/x [ProductCode]")),

                     new Dir(@"%Desktop%",
                        new ExeFileShortcut("MyApp", "[INSTALL_DIR]MyApp.exe", "")
                        {
                            Condition = new Condition("INSTALLDESKTOPSHORTCUT=\"yes\"") //property based condition
                        }),

                    //setting property to be used in install condition
                    new Property("INSTALLDESKTOPSHORTCUT", "no"),
                    new Property("ALLUSERS", "1"),
                    new ManagedAction(@"MyAction", Return.ignore, When.Before, Step.LaunchConditions, Condition.NOT_Installed, Sequence.InstallUISequence));


        project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b");
        project.UI = WUI.WixUI_ProgressOnly;
        project.OutFileName = "setup";

        Compiler.BuildMsi(project);
    }
}

public class CustonActions
{
    [CustomAction]
    public static ActionResult MyAction(Session session)
    {
        if (DialogResult.Yes == MessageBox.Show("Do you want to install desktop shortcut", "Installation", MessageBoxButtons.YesNo))
            session["INSTALLDESKTOPSHORTCUT"] = "yes";

        return ActionResult.Success;
    }
}


