using Fabric.IdentityProviderSearchService.Services;
using Nancy;
using Nancy.TinyIoc;

namespace Fabric.IdentityProviderSearchService
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
            container.Register<IExternalIdentityProviderService, ActiveDirectoryProviderService>();
            container.Register<PrincipalSeachService, PrincipalSeachService>();
            
        }
    }
}