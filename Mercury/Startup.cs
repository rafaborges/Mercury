using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Mercury.Startup))]
namespace Mercury
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }

    }
}