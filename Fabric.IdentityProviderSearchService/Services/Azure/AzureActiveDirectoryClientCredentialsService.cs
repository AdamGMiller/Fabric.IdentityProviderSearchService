using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

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
        
        public async Task<string> GetAzureAccessTokenAsync(string tenantId)
        {
            // Could not find an instance where this is multiple.  Leaving code the room for this.
            var scopes = applicationConfiguration.AzureActiveDirectoryClientSettings.Scopes;
            if (scopes != null && scopes.Count() <= 0)
            {
                throw new AzureActiveDirectoryException(Resource.NoScopesDefinedForAzureAD);
            }

            // TODO : clean up strings and add to constants file
            var content = new List<KeyValuePair<string, string>>()
            {
               new KeyValuePair<string, string>( "client_id", applicationConfiguration.AzureActiveDirectoryClientSettings.ClientId ),
               new KeyValuePair<string, string>( "client_secret", applicationConfiguration.AzureActiveDirectoryClientSettings.ClientSecret ),
               new KeyValuePair<string, string>( "scope", applicationConfiguration.AzureActiveDirectoryClientSettings.Scopes.First() ),  // not sure how to append multiple.  Only looking for first one for now.
               new KeyValuePair<string, string>( "grant_type", "client_credentials" )
            };

            string url = $"{applicationConfiguration.AzureActiveDirectoryClientSettings.Authority.TrimEnd('/')}/{tenantId}/oauth2/v2.0/token";
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(content) };

            var response = await client.SendAsync(request).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<string>(responseContent);
            }

            // TODO: add to resource file
            string exceptionMessage = $"Could not retrieve Azure access Token.  Here is the exception: {responseContent}";
            throw new AzureActiveDirectoryException(exceptionMessage);
        }

        public string[] GetTenants()
        {
            return applicationConfiguration.AzureActiveDirectoryClientSettings.IssuerWhiteList;
        }
    }
}