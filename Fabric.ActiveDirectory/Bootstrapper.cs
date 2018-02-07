using Fabric.ActiveDirectory.Services;
using Nancy;
using Nancy.TinyIoc;

namespace Fabric.ActiveDirectory
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