using Microsoft.Owin.Hosting;
using System;
using System.ServiceProcess;

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
            var baseAddress = "http://*:4433";

            var url = ExchangeServiceProvider.Url;

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
