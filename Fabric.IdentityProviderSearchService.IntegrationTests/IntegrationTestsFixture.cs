using System.Security.Claims;
using Fabric.IdentityProviderSearchService.Configuration;
using Moq;
using Nancy.Testing;
using Serilog;


namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public class IntegrationTestsFixture
    {
        public Browser GetBrowser(ClaimsPrincipal principal)
        {
            var appConfiguration = new AppConfiguration
            {
               DomainName = "testing",
               UseWindowsAuthentication = true
            };

            var bootstrapper = new TestBootstrapper(appConfiguration, new Mock<ILogger>().Object, principal);

            return new Browser(bootstrapper, context =>
            {
                context.HostName("testhost");
                context.Header("Content-Type", "application/json");
                context.Header("Accept", "application/json");
            });
        }
    }
}
