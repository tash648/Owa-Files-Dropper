using System;
using System.Linq;
using System.ServiceProcess;

namespace OwaAttachmentServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            new OwaFilesListener().Start();
            Console.ReadLine();

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new OwaFilesListener()
            //};
            //ServiceBase.Run(ServicesToRun);
        }
    }
}
