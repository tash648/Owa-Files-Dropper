//css_ref ..\..\..\..\WixSharp.dll;
//css_ref System.Core.dll;
//css_ref ..\..\..\..\Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;

using System;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;

class Script
{
    static public void Main(string[] args)
    {
        var project = new Project()
        {
            UI = WUI.WixUI_ProgressOnly,
            Name = "CustomActionTest",

            Actions = new[]
            {
                new ManagedAction("MyAction", "%this%")
            }
        };

        Compiler.BuildMsi(project);
    }
}

public class CustomActions
{
    [CustomAction]
    public static ActionResult MyAction(Session session)
    {
#if DEBUG
        System.Diagnostics.Debugger.Launch();
#endif
        session.Log("Begin CustomAction2 Hello World");

        return ActionResult.Success;
    }
}



