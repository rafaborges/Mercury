using log4net;
using log4net.Config;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.TinyIoc;

namespace Aphrodite.Server
{
    class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override IRootPathProvider RootPathProvider
        {
            get { return new RootPathProvider(); }
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);

            conventions.StaticContentsConventions.AddDirectory("Scripts");
            conventions.StaticContentsConventions.AddDirectory("fonts");
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            XmlConfigurator.Configure();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(typeof(ILog), (c, o) => LogManager.GetLogger(typeof(Bootstrapper)));
        }

        public override void Configure(INancyEnvironment environment)
        {
            base.Configure(environment);
            environment.Diagnostics(password: "!995DixonBay!");
        }
    }
}
