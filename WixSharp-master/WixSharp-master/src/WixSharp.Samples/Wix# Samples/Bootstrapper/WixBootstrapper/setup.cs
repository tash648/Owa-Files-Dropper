//css_ref ..\..\..\WixSharp.dll;
//css_ref System.Core.dll;
//css_ref System.Xml.dll;
//css_ref System.Xml.Linq.dll;
//css_ref ..\..\..\Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
using System;
using System.Xml;
using io = System.IO;
using WixSharp;
using WixSharp.Bootstrapper;
using WixSharp.CommonTasks;

public class InstallScript
{
    static public void Main(string[] args)
    {
        var productProj =
            new Project("My Product",
                new Dir(@"%ProgramFiles%\My Company\My Product",
                    new File("readme.txt")))
            { InstallScope = InstallScope.perUser };
        //---------------------------------------------------------
        var crtProj =
            new Project("CRT",
                new Dir(@"%ProgramFiles%\My Company\CRT",
                    new File("readme.txt")))
            { InstallScope = InstallScope.perUser };

        //---------------------------------------------------------
        string productMsi = productProj.BuildMsi();
        string crtMsi = crtProj.BuildMsi();        
        //---------------------------------------------------------

        var bootstrapper =
                new Bundle("My Product",
                    new PackageGroupRef("NetFx40Web"),
                    new MsiPackage(crtMsi) { DisplayInternalUI = true },
                    new MsiPackage(productMsi) { DisplayInternalUI = true });

        bootstrapper.AboutUrl = "https://wixsharp.codeplex.com/";
        bootstrapper.IconFile = "app_icon.ico";
        bootstrapper.Version = new Version("1.0.0.0");
        bootstrapper.UpgradeCode = new Guid("6f330b47-2577-43ad-9095-1861bb25889b");
        bootstrapper.Application.LogoFile = "logo.png";
        bootstrapper.Application.LicensePath = "licence.html";  //HyperlinkLicense app with embedded license file
        //bootstrapper.Application.LicensePath = "licence.rtf"; //RtfLicense app with embedded license file 
        //bootstrapper.Application.LicensePath = "http://opensource.org/licenses/MIT"; //HyperlinkLicense app with online license file
        //bootstrapper.Application.LicensePath = null; //HyperlinkLicense app with no license

        bootstrapper.PreserveTempFiles = true;
        bootstrapper.Build();
        //---------------------------------------------------------

        //if (io.File.Exists(productMsi))
          //  io.File.Delete(productMsi);

        if (io.File.Exists(crtMsi))
            io.File.Delete(crtMsi);
    }
}

