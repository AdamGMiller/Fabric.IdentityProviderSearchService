using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public class AzureActiveDirectoryClientCredentialsService : IAzureActiveDirectoryClientCredentialsService
    {
        private IAppConfiguration applicationConfiguration;
        private HttpClient client;

        public AzureActiveDirectoryClientCredentialsService(IAppConfiguration appConfig, HttpClient client)
        {
            this.applicationConfiguration = appConfig;
            this.client = client;
        }
        
        public async Task<AzureActiveDirectoryResponse> GetAzureAccessTokenAsync(string tenantId)
        {
            // Could not find an instance where this is multiple.  Leaving code the room for this.
            var scopes = applicationConfiguration.AzureActiveDirectoryClientSettings.Scopes;
            if (scopes != null && scopes.Count() <= 0)
            {
                throw new AzureActiveDirectoryException(Resource.NoScopesDefinedForAzureAD);
            }

            var content = new List<KeyValuePair<string, string>>()
            {
               new KeyValuePair<string, string>( AzureRequestHeaders.ClientId, applicationConfiguration.AzureActiveDirectoryClientSettings.ClientId ),
               new KeyValuePair<string, string>( AzureRequestHeaders.ClientSecret, applicationConfiguration.AzureActiveDirectoryClientSettings.ClientSecret ),
               new KeyValuePair<string, string>( AzureRequestHeaders.Scope, applicationConfiguration.AzureActiveDirectoryClientSettings.Scopes.First() ),  // not sure how to append multiple.  Only looking for first one for now.
               new KeyValuePair<string, string>( AzureRequestHeaders.GrantType, "client_credentials" )
            };

            string url = $"{applicationConfiguration.AzureActiveDirectoryClientSettings.Authority.TrimEnd('/')}/{tenantId}/oauth2/v2.0/token";
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(content) };

            var response = await client.SendAsync(request).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<AzureActiveDirectoryResponse>(responseContent);
            }

            string exceptionMessage = $"Could not retrieve Azure access Token.  The response was: {responseContent}";
            throw new AzureActiveDirectoryException(exceptionMessage);
        }

        public string[] GetTenants()
        {
            return applicationConfiguration.AzureActiveDirectoryClientSettings.IssuerWhiteList;
        }
    }
}