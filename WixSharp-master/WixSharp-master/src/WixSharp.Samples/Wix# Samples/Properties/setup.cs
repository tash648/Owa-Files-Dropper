//css_dir ..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref System.Core.dll;

using System;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.CommonTasks;

class Script
{
    static public void Main(string[] args)
    {
        var project = new Project()
        {
            UI = WUI.WixUI_ProgressOnly,
            Name = "PropertiesTest",

            Dirs = new[] { new Dir(@"%ProgramFiles%\PropertiesTest") },

            Actions = new WixSharp.Action[] 
            { 
                new ManagedAction("ShowGritting"), 
                new QtCmdLineAction("notepad.exe", "[NOTEPAD_FILE]"),
            },

            Properties = new[] 
            { 
                new Property("Gritting", "Hello World!"),
                new Property("Title", "Properties Test"),
                new Property("NOTEPAD_FILE", @"C:\boot.ini")
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
            //accessing property with SQL
            var message = (string)session.Database.ExecuteScalar(
                "SELECT `Value` FROM `Property` WHERE `Property` = 'Gritting'");

            //accessing property with Session object
            MessageBox.Show(message, session["Title"]);

            MessageBox.Show("The product is installed in: " + session["INSTALLDIR"], session["Title"]);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString(), "Error");
        }
        return ActionResult.Success;
    }
}



