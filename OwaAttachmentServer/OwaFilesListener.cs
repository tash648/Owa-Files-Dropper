using System;
using System.ServiceProcess;
using Microsoft.Owin.Hosting;
using System.Configuration;
using System.Diagnostics;

namespace OwaAttachmentServer
{
    public partial class OwaFilesListener : ServiceBase
    {
        private IDisposable service;

        public OwaFilesListener()
        {
            InitializeComponent();
        }

        public void Start()
        {
            var baseAddress = "https://*:8080";            

            var options = new StartOptions(baseAddress);

            service = WebApp.Start<Startup>(options);            
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
