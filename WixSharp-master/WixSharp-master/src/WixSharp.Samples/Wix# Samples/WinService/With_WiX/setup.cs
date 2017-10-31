//css_dir ..\..\..\;
//css_ref Wix_bin\SDK\Microsoft.Deployment.WindowsInstaller.dll;
//css_ref System.Core.dll;

using System;
using System.Data;
using System.Linq;
using WixSharp;

class Script
{
    static public void Main(string[] args)
    {
        try
        {
            File service;
            
            var project =
                new Project("My Product",
                    new Dir(@"%ProgramFiles%\My Company\My Product",
                        service = new File(@"..\SimpleService\MyApp.exe")));

            //The service file element can also be located as in the following commented code
            //File service = project.FindFile(f => f.Name.EndsWith("MyApp.exe"));
            //File service = project.AllFiles.Single(f => f.Name.EndsWith("MyApp.exe"));

            service.ServiceInstaller = new ServiceInstaller
                                       {
                                           Name = "WixSharp.TestSvc",
                                           DependsOn = "Dnscache;Dhcp",
                                           StartOn = SvcEvent.Install,
                                           StopOn = SvcEvent.InstallUninstall_Wait,
                                           RemoveOn = SvcEvent.Uninstall_Wait,
                                           DelayedAutoStart = true,
                                           ServiceSid = ServiceSid.none,
                                           FirstFailureActionType = FailureActionType.restart,
                                           SecondFailureActionType = FailureActionType.restart,
                                           ThirdFailureActionType = FailureActionType.runCommand,
                                           ProgramCommandLine = "MyApp.exe -run",
                                           RestartServiceDelayInSeconds = 30,
                                           ResetPeriodInDays = 1,
                                           PreShutdownDelay = 1000 * 60 * 3,
                                           RebootMessage = "Failure actions do not specify reboot",
                                       };

            project.GUID = new Guid("6fe30b47-2577-43ad-9195-1861ba25889b");
            project.OutFileName = "setup";

            project.PreserveTempFiles = true;
            project.BuildMsi();            
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
