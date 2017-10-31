using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WixSharp;
using WixSharp.UI.Forms;

internal static class Defaults
{
    public const string UserName = "MP_USER";
}

public class Script
{

    static public void Main()
    {
        var project = new ManagedProject("ManagedSetup",
                          new User
                          {
                              Name = Defaults.UserName,
                              Password = "[PASSWORD]",
                              Domain = "[DOMAIN]",
                              PasswordNeverExpires = true,
                              CreateUser = true
                          },
                          new Property("PASSWORD", "pwd123"));

        project.SourceBaseDir = @"..\..\";
        project.GUID = new Guid("6f330b47-2577-43ad-9095-1861ba25889b");
        project.LocalizationFile = "MyProduct.en-us.wxl";

        project.ManagedUI = new ManagedUI();
        project.ManagedUI.InstallDialogs.Add<WelcomeDialog>()
                                        .Add<MyProduct.UserNameDialog>()
                                        .Add<ProgressDialog>()
                                        .Add<ExitDialog>();

        //it effectively becomes a 'Repair' sequence 
        project.ManagedUI.ModifyDialogs.Add<ProgressDialog>()
                                       .Add<ExitDialog>();

        project.UILoaded += msi_UILoaded;
        project.BeforeInstall += msi_BeforeInstall;

        project.BuildMsi();
    }

    static void msi_UILoaded(SetupEventArgs e)
    {
        //You can set the size of the shell view window if requred
        //e.ManagedUIShell.SetSize(700, 500);
    } 

    static void msi_BeforeInstall(SetupEventArgs e)
    {
        //Note: the property will not be from UserNameDialog if MSI UI is suppressed
        if (e.Session["DOMAIN"] == null)
            e.Session["DOMAIN"] = Environment.MachineName;
    }
}
