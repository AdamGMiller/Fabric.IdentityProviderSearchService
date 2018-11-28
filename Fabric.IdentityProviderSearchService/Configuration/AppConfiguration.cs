using Fabric.Platform.Shared.Configuration;

namespace Fabric.IdentityProviderSearchService.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        public string DomainName { get; set; }

        public string DiscoveryServiceEndpoint { get; set; }

        public bool UseDiscoveryService { get; set; }

        public bool UseAzureAuthentication { get; set; }

        public bool UseWindowsAuthentication { get; set; }

        public ApplicationInsights ApplicationInsights { get; set; }

        public IdentityServerConfidentialClientSettings IdentityServerConfidentialClientSettings { get; set; }

        public AzureActiveDirectoryClientSettings AzureActiveDirectoryClientSettings { get; set; }

        public EncryptionCertificateSettings EncryptionCertificateSettings { get; set; }
    }
}