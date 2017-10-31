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
        var project = new Project()
        {
            UI = WUI.WixUI_ProgressOnly,
            Name = "PropertiesTest",

            Actions = new WixSharp.Action[] 
            { 
                new SetPropertyAction("Gritting", "Hello World!"),
                new SetPropertyAction("Title", "SetProperties Test"),
                new SetPropertyAction("NOTEPAD_FILE", @"C:\boot.ini"),

                new ManagedAction(@"ShowGritting"), 
                new QtCmdLineAction("notepad.exe", "[NOTEPAD_FILE]"),
            },
            Properties = new[] 
            { 
                new Property("Gritting", "empty"),
                new Property("Title", "empty"),
                new Property("NOTEPAD_FILE", "empty")
            }
        };

        Compiler.BuildMsi(project);
    }
}

public class CustonActions
{
    [CustomAction]
    public static ActionResult ShowGritting(Session session)
    {
        try
        {
            MessageBox.Show(session["Gritting"], session["Title"]);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString(), "Error");
        }
        return ActionResult.Success;
    }
}



