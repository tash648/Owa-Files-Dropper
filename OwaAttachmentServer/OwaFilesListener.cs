using System;
using System.ServiceProcess;
using Microsoft.Owin.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.ComponentModel;

namespace OwaAttachmentServer
{
    public partial class OwaFilesListener : ServiceBase
    {
        private IDisposable service;

        public OwaFilesListener()
        {
            InitializeComponent();

            //Setup logging
            this.AutoLog = false;

            ((ISupportInitialize)this.EventLog).BeginInit();

            if (!EventLog.SourceExists(this.ServiceName))
            {
                EventLog.CreateEventSource(this.ServiceName, "Application");
            }

            ((ISupportInitialize)this.EventLog).EndInit();

            this.EventLog.Source = this.ServiceName;
            this.EventLog.Log = "Application";
        }

        public void Start()
        {
            var baseAddress = "https://*:8080";            

            var options = new StartOptions(baseAddress);

            service = WebApp.Start<Startup>(options);

            this.EventLog.WriteEntry("ExportFolder " + ConfigurationManager.AppSettings["ExportFolder"]);
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        protected override void OnStop()
        {
            service?.Dispose();
        }
    }
}
