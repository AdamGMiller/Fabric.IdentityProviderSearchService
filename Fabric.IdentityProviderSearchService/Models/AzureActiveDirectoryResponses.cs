using Newtonsoft.Json;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class AzureActiveDirectoryResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }
    }
}