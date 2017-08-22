using Microsoft.Owin;
using Nancy.Owin;
using Owin;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(Aphrodite.Server.Startup))]
namespace Aphrodite.Server
{
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //var listener = (HttpListener)app.Properties["System.Net.HttpListener"];
            //listener.AuthenticationSchemes = AuthenticationSchemes.IntegratedWindowsAuthentication;

            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
            var options = new NancyOptions()
            {
                Bootstrapper = new Bootstrapper()
            };
            app.UseNancy(options);
        }
    }
}