namespace Fabric.IdentityProviderSearchService.Configuration
{
    using System.Threading.Tasks;

    using Fabric.IdentityProviderSearchService.Services;

    /// <summary>
    /// Extension class for IAppConfiguration extensions.
    /// </summary>
    public static class AppConfigurationExtensions
    {
        /// <summary>
        /// Configures the IdentityService Url in IAppConfiguration.
        /// </summary>
        /// <param name="appConfig">The <see cref="IAppConfiguration"/> instance to configure.</param>
        public static void ConfigureIdentityServiceUrl(this IAppConfiguration appConfig)
        {
            if (!appConfig.UseDiscoveryService)
            {
                return;
            }

            using (var discoveryServiceClient = new DiscoveryServiceClient(appConfig.DiscoveryServiceEndpoint))
            {
                var identityServiceRegistration = Task
                    .Run(() => discoveryServiceClient.GetServiceAsync("IdentityService", 1))
                    .Result;

                if (!string.IsNullOrEmpty(identityServiceRegistration?.ServiceUrl))
                {
                    appConfig.IdentityServerConfidentialClientSettings.Authority =
                        identityServiceRegistration.ServiceUrl;
                }
            }
        }
    }
}