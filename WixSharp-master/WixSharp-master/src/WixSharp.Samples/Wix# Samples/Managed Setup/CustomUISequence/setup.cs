//css_dir ..\..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref WixSharp.UI.dll;
//css_ref System.Core.dll;
//css_ref System.Xml.dll;
using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Forms;
using WixSharp.UI.Forms;

public class Script
{
    static public void Main()
    {
        var binaries = new Feature("Binaries", "Product binaries", true, false);
        var docs = new Feature("Documentation", "Product documentation (manuals and user guides)", true);
        var tuts = new Feature("Tutorials", "Product tutorials", false);
        docs.Children.Add(tuts);

        var project = new ManagedProject("ManagedSetup",
                            new Dir(@"%ProgramFiles%\My Company\My Product",
                                new File(binaries, @"..\Files\bin\MyApp.exe"),
                                new Dir("Docs",
                                    new File(docs, "readme.txt"),
                                    new File(tuts, "setup.cs"))));

        project.ManagedUI = new ManagedUI();

        //project.MinimalCustomDrawing = true;

        project.UIInitialized += CheckCompatibility; //will be fired on the embedded UI start
        project.Load += CheckCompatibility;          //will be fired on the MSI start

        //removing all entry dialogs and installdir
        project.ManagedUI.InstallDialogs//.Add(Dialogs.Welcome)
                                        //.Add(Dialogs.Licence)
                                        //.Add(Dialogs.SetupType)
                                        .Add(Dialogs.Features)
                                        //.Add(Dialogs.InstallDir)
                                        .Add(Dialogs.Progress)
                                        .Add(Dialogs.Exit);

        //removing entry dialog
        project.ManagedUI.ModifyDialogs//.Add(Dialogs.MaintenanceType)
                                        .Add(Dialogs.Features)
                                        .Add(Dialogs.Progress)
                                        .Add(Dialogs.Exit);

        project.GUID = new Guid("6f330b47-2577-43ad-9095-1861ba25889b");

        project.ControlPanelInfo.InstallLocation = "[INSTALLDIR]";
        //project.ControlPanelInfo.InstallLocation = @"C:\";

        project.SourceBaseDir = @"..\..\";
        project.BuildMsi();
    }

    static void CheckCompatibility(SetupEventArgs e)
    {
        //MessageBox.Show("Hello World! (CLR: v" + Environment.Version + ")", "Embedded Managed UI (" + ((IntPtr.Size == 8) ? "x64" : "x86") + ")");

        if (e.IsInstalling)
        {
            var conflictingProductCode = "{1D6432B4-E24D-405E-A4AB-D7E6D088C111}";

            if (AppSearch.IsProductInstalled(conflictingProductCode))
            {
                string msg = string.Format("Installed '{0}' is incompatible with this product.\n" +
                                           "Setup will be aborted.",
                                           AppSearch.GetProductName(conflictingProductCode) ?? conflictingProductCode);
                MessageBox.Show(msg, "Setup");
                e.Result = ActionResult.UserExit;
            }
        }
    }
}
