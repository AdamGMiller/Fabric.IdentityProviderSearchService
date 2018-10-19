namespace Fabric.IdentityProviderSearchService.Configuration
{
    public class AzureActiveDirectoryClientSettings
    {
        public AzureActiveDirectoryClientSettings()
        {

        }

        public AzureClientApplicationSettings[] ClientAppSettings { get; set; }

        public string Authority { get; set; }

        public string TokenEndpoint { get; set; }
    }
}