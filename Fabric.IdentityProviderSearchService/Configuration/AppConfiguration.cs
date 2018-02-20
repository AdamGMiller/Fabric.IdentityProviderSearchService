using Fabric.Platform.Shared.Configuration;

namespace Fabric.IdentityProviderSearchService.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        public string DomainName { get; set; }
        public ApplicationInsights ApplicationInsights { get; set; }
        public IdentityServerConfidentialClientSettings IdentityServerConfidentialClientSettings { get; set; }
    }
}