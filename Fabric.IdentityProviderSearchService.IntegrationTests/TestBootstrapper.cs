using System.Security.Claims;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Services;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Moq;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Serilog;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public class TestBootstrapper : Bootstrapper
    {
        private readonly ClaimsPrincipal _claimsPrincipal;

        public TestBootstrapper(IAppConfiguration appConfig, ILogger logger, ClaimsPrincipal claimsPrincipal) 
            : base(appConfig, logger)
        {
            _claimsPrincipal = claimsPrincipal;
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            context.CurrentUser = _claimsPrincipal;
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            var mockProxy = new Mock<IActiveDirectoryProxy>()
                .SetupActiveDirectoryProxy(new ActiveDirectoryDataHelper().GetPrincipals());
            container.Register(mockProxy.Object);

            var mockGraph = new Mock<IMicrosoftGraphApi>()
                .SetupAzureDirectoryGraph(new ActiveDirectoryDataHelper().GetMicrosoftGraphGroups());
            container.Register(mockGraph.Object);
        }
    }
}
