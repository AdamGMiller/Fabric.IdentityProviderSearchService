using Fabric.Platform.Shared.Configuration;

namespace Fabric.IdentityProviderSearchService.Configuration
{
    public interface IAppConfiguration
    {
        string DomainName { get; set; }

        string DiscoveryServiceEndpoint { get; set; }

        bool UseDiscoveryService { get; set; }

        ApplicationInsights ApplicationInsights { get; set; }

        IdentityServerConfidentialClientSettings IdentityServerConfidentialClientSettings { get; set; }
    }
}
