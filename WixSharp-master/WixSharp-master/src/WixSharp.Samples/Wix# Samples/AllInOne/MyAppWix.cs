//css_dir ..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref System.Core.dll;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using WixSharp;
using WixSharp.CommonTasks;

class Script
{
    static public void Main(string[] args)
    {
        try
        {
            Feature binaries = new Feature("MyApp Binaries");
            Feature docs = new Feature("MyApp Documentation");

            Project project =
                new Project("My Product",

                    //Files and Shortcuts
                    new Dir(@"%ProgramFiles%\My Company\My Product",
                        new File(binaries, @"AppFiles\MyApp.exe",
                            new FileShortcut(binaries, "MyApp", @"%ProgramMenu%\My Company\My Product"),
                            new FileShortcut(binaries, "MyApp", @"%Desktop%")),
                        new File(binaries, @"AppFiles\Registrator.exe"),
                        new File(docs, @"AppFiles\Readme.txt"),
                        new File(binaries, @"AppFiles\MyApp.ico"),
                        new ExeFileShortcut(binaries, "Uninstall MyApp", "[System64Folder]msiexec.exe", "/x [ProductCode]")),

                    new Dir("%Startup%",
                        new ExeFileShortcut(binaries, "MyApp", "[INSTALLDIR]MyApp.exe", "")),

                    new Dir(@"%ProgramMenu%\My Company\My Product",
                        new ExeFileShortcut(binaries, "Uninstall MyApp", "[System64Folder]msiexec.exe", "/x [ProductCode]")),

                    //Registries
                    new RegValue(binaries, RegistryHive.LocalMachine, @"Software\My Product", "ExePath", @"[INSTALLDIR]MyApp.exe"),

                    //Custom Actions
                    new InstalledFileAction("Registrator.exe", "", Return.check, When.After, Step.InstallExecute, Condition.NOT_Installed),
                    new InstalledFileAction("Registrator.exe", "/u", Return.check, When.Before, Step.InstallExecute, Condition.Installed),

                    new ScriptAction(@"MsgBox ""Executing VBScript code...""", Return.ignore, When.After, Step.InstallFinalize, Condition.NOT_Installed),

                    new ScriptFileAction(@"CustomActions\Sample.vbs", "Execute", Return.ignore, When.After, Step.InstallFinalize, Condition.NOT_Installed),

                    new PathFileAction(@"%WindowsFolder%\notepad.exe", "readme.txt", @"INSTALLDIR", Return.asyncNoWait, When.After, Step.InstallFinalize, Condition.NOT_Installed),

                    new ManagedAction(@"MyManagedAction", "%this%"),

                    new InstalledFileAction("MyApp.exe", ""));


            project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b"); // or project.Id = Guid.NewGuid();   
            project.LicenceFile = @"AppFiles\License.rtf";
            project.UI = WUI.WixUI_Mondo;
            project.SourceBaseDir = Environment.CurrentDirectory;
            project.OutFileName = "MyApp";
            
            //project.PreserveTempFiles = true;
            project.WixSourceGenerated += Compiler_WixSourceGenerated;
            project.BuildMsi();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void Compiler_WixSourceGenerated(System.Xml.Linq.XDocument document)
    {
        document.Root.Descendants("Shortcut")
                     .ToList()
                     .ForEach(x => 
                      {
                          if(x.Attribute("Name").Value == "MyApp.lnk")
                            x.Attribute("Name").Value = "My Product App.lnk";
                      });
    }
}

public class CustomActions
{
    [CustomAction]
    public static ActionResult MyManagedAction(Session session)
    {
        MessageBox.Show("Hello World!", "Embedded Managed CA");

        return ActionResult.Success;
    }
}


