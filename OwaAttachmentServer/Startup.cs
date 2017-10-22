using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Owin;
using System.Web.Http;

namespace OwaAttachmentServer
{
    public class Startup 
    { 
        public void Configuration(IAppBuilder app) 
        { 
            var config = new HttpConfiguration(); 
            config.Routes.MapHttpRoute( 
                name: "Owa api", 
                routeTemplate: "api/{controller}/{action}/{id}", 
                defaults: new { id = RouteParameter.Optional } 
            );

            app.UseWebApi(config);

            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    EnableDetailedErrors = true,
                    EnableJavaScriptProxies = true
                };

                map.RunSignalR(hubConfiguration);
            });
        }
    } 
}
