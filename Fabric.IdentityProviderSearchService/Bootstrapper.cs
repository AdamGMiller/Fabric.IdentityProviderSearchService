using System;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Infrastructure.PipelineHooks;
using Fabric.IdentityProviderSearchService.Services;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Fabric.IdentityProviderSearchService
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly IAppConfiguration _appConfig;

        public Bootstrapper(IAppConfiguration appConfig)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            pipelines.BeforeRequest += ctx => RequestHooks.RemoveContentTypeHeaderForGet(ctx);
            pipelines.BeforeRequest += ctx => RequestHooks.SetDefaultVersionInUrl(ctx);

            container.Register(_appConfig);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
            container.Register<IExternalIdentityProviderService, ActiveDirectoryProviderService>();
            container.Register<PrincipalSeachService, PrincipalSeachService>();
            
        }
    }
}