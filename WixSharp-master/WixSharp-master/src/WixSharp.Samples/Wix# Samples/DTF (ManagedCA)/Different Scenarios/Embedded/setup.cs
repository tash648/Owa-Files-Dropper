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

public class CustonActions
{
    [CustomAction]
    public static ActionResult MyAction(Session session)
    {
        MessageBox.Show("Hello World!", "Embedded Managed CA");
        session.Log("Begin MyAction Hello World");

        return ActionResult.Success;
    }
}



