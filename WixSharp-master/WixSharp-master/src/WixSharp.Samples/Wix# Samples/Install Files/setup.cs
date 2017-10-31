//css_dir ..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref System.Core.dll;
//css_ref System.Xml.dll;
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using WixSharp;
using WixSharp.CommonTasks;

class Script
{
    static public void Main(string[] args)
    {
        var project =
            new Project("MyProduct",
                new Dir(@"%ProgramFiles%\My Company\My Product",
                    new File(@"Files\Bin\MyApp.exe"),
                    new Dir(@"Docs\Manual",
                        new File(@"Files\Docs\Manual.txt") { NeverOverwrite = true })));

        project.UI = WUI.WixUI_InstallDir;
        project.GUID = new Guid("6f330b47-2577-43ad-9095-1861ba25889b");
        project.EmitConsistentPackageId = true;
        project.PreserveTempFiles = true;

        Compiler.WixSourceGenerated += Compiler_WixSourceGenerated;
        Compiler.BuildMsi(project);
    }

    static void Compiler_WixSourceGenerated(XDocument document)
    {
        //Will make MyApp.exe directory writable.
        //It is actually a bad practice to write to program files and this code is provided for sample purposes only.
        document.FindAll("Component")
                .Single(x => x.HasAttribute("Id", value=>value.EndsWith("MyApp.exe")))
                .AddElement("CreateFolder/Permission", "User=Everyone;GenericAll=yes");
    }
}



