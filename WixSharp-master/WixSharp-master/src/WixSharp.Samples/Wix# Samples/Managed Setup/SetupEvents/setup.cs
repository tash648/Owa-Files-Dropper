//css_dir ..\..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref WixSharp.UI.dll;
//css_ref System.Core.dll;
//css_ref System.Xml.dll;

using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Linq;
using System.Windows.Forms;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Forms;
using WixSharp.UI.Forms;

public class Script
{
    static public void Main()
    {
        var project =
            new ManagedProject("ManagedSetup",
                new Dir(@"%ProgramFiles%\My Company\My Product",
                    new File(@"..\Files\bin\MyApp.exe"),
                    new Dir("Docs",
                        new File("readme.txt"))));

        project.ManagedUI = ManagedUI.Empty;

        project.UIInitialized += project_UIInit;
        project.Load += project_Load;
        project.BeforeInstall += project_BeforeInstall;
        project.AfterInstall += project_AfterInstall;

        project.GUID = new Guid("6f330b47-2577-43ad-9095-1861ba25889b");

        Compiler.BuildMsi(project);
    }

    static void project_UIInit(SetupEventArgs e)
    {
        SetEnvVErsion(e.Session);
    }

    static void SetEnvVErsion(Session session)
    {
        if(session["EnvVersion"].IsEmpty())
            session["EnvVersion"] = AppSearch.IniFileValue(Environment.ExpandEnvironmentVariables(@"%windir%\win.ini"),
                                                           "System",
                                                           "Version") ?? "<unknown>";
    }

    static void project_Load(SetupEventArgs e)
    {
        try
        {
            SetEnvVErsion(e.Session);

            if (string.IsNullOrEmpty(e.Session["INSTALLDIR"])) //installdir is not set yet
            {
                string installDirProperty = e.Session.Property("WixSharp_UI_INSTALLDIR");
                string defaultinstallDir = e.Session.GetDirectoryPath(installDirProperty);

                e.Session["INSTALLDIR"] = System.IO.Path.Combine(defaultinstallDir, Environment.UserName);
            }
        }
        catch { }
        MessageBox.Show(e.ToString(), "Load " + e.Session["EnvVersion"]);
    }

    static void project_BeforeInstall(SetupEventArgs e)
    {
        MessageBox.Show(e.ToString(), "BeforeInstall");
    }

    static void project_AfterInstall(SetupEventArgs e)
    {
        MessageBox.Show(e.ToString(), "AfterExecute");
    }
}
