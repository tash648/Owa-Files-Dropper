//css_dir ..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref System.Core.dll;
//css_ref System.Xml.dll;
using System;
using System.Configuration;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.CommonTasks;
using IO = System.IO;

class Script
{
    static public void Main()
    {
        try
        {
            var project =
                new Project("My Product",
                    new Dir(@"%ProgramFiles%\My Company\My Product",
                        new File(@"Files\MyApp.exe"),
                        new File(@"Files\MyApp.exe.config")),
                new ElevatedManagedAction("OnInstall", Return.check, When.After, Step.InstallFiles, Condition.NOT_Installed)
                {
                    UsesProperties = "CONFIG_FILE=[INSTALLDIR]MyApp.exe.config, APP_FILE=[INSTALLDIR]MyApp.exe"
                });

            project.GUID = new Guid("6fe30b47-2577-43ad-9195-1861ba25889b");

            //Compiler.PreserveTempFiles = true;
            Compiler.BuildMsi(project);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

public class CustomActions
{
    [CustomAction]
    public static ActionResult OnInstall(Session session)
    {
        //Note if your custom action requires non-GAC assembly then you need deploy it too.
        //You can do it by setting ManagedAction.RefAssemblies.
        //See "Wix# Samples\DTF (ManagedCA)\Different Scenarios\ExternalAssembly" sample for details.

        //System.Diagnostics.Debugger.Launch();

        session.Log("------------- " + session.Property("INSTALLDIR"));
        session.Log("------------- " + session.Property("CONFIG_FILE"));
        session.Log("------------- " + session.Property("APP_FILE"));

        return session.HandleErrors(() =>
        {
            string configFile = session.Property("INSTALLDIR") + "MyApp.exe.config";
            
            //alternative ways of extracting 'deferred' properties
            //configFile = session.Property("APP_FILE") + ".config"; 
            //configFile = session.Property("CONFIG_FILE");

            UpdateAsAppConfig(configFile);

            //alternative implementations for the config manipulations
            UpdateAsXml(configFile);
            UpdateAsText(configFile);
            UpdateWithWixSharp(configFile);

            MessageBox.Show(GetContext());
        });

    }

    static string GetContext()
    {
        if (WindowsIdentity.GetCurrent().IsAdmin())
            return "Admin User";
        else
            return "Normal User";
    }

    static public void UpdateAsAppConfig(string configFile)
    {
        var config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = configFile }, ConfigurationUserLevel.None);

        config.AppSettings.Settings["AppName"].Value = "My App";

        var section = config.ConnectionStrings;
        section.ConnectionStrings["Server1"].ConnectionString = "DataSource=(localdb)/v11.0;IntegratedSecurity=true";
        section.ConnectionStrings["Server1"].ProviderName = "System.Data.SqlClient";

        config.Save();
    }

    static public void UpdateAsXml(string configFile)
    {
        var config = XDocument.Load(configFile);

        config.XPathSelectElement("//configuration/appSettings/add[@key='AppName']").Attribute("value").Value = "My App";
        config.XPathSelectElement("//configuration/connectionStrings/add[@name='Server1']").Attribute("connectionString").Value = "DataSource=(localdb)/v11.0;IntegratedSecurity=true";
        config.XPathSelectElement("//configuration/connectionStrings/add[@name='Server1']").Attribute("providerName").Value = "System.Data.SqlClient";

        config.Save(configFile);
    }

    static public void UpdateAsText(string configFile)
    {
        string configuration = IO.File.ReadAllText(configFile);

        configuration = configuration.Replace("{$AppName}", "My App")
                                     .Replace("{$ConnectionString}", "DataSource=(localdb)/v11.0;IntegratedSecurity=true")
                                     .Replace("{$ProviderName}", "System.Data.SqlClient");

        IO.File.WriteAllText(configFile, configuration);
    }

    static public void UpdateWithWixSharp(string configFile)
    {
        Tasks.SetConfigAttribute(configFile, "//configuration/appSettings/add[@key='AppName']/@value", "My App");
        Tasks.SetConfigAttribute(configFile, "//configuration/connectionStrings/add[@name='Server1']/@connectionString", "DataSource=(localdb)/v11.0;IntegratedSecurity=true");
        Tasks.SetConfigAttribute(configFile, "//configuration/connectionStrings/add[@name='Server1']/@providerName", "System.Data.SqlClient");
    }
}