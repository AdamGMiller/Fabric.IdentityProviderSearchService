using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Exceptions;
using IdentityModel.Client;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public class AzureActiveDirectoryClientCredentialsService : IAzureActiveDirectoryClientCredentialsService
    {
        private HttpClient _client;
        private string _authority;
        private string _tokenEndpoint;
        private IDictionary<string, AzureClientApplicationSettings> _settings;

        public AzureActiveDirectoryClientCredentialsService(AzureActiveDirectoryClientSettings azureClientSettings, HttpClient client)
        {
            this._settings = AzureClientApplicationSettings.CreateDictionary(azureClientSettings);
            this._authority = azureClientSettings.Authority;
            this._tokenEndpoint = azureClientSettings.TokenEndpoint;
            this._client = client;
        }

        public async Task<TokenResponse> GetAzureAccessTokenAsync(string tenantId)
        {
            var appSettings = this._settings[tenantId];
            TokenClient tokenClient = new TokenClient($"{this._authority.TrimEnd('/')}/{tenantId}/{_tokenEndpoint}", appSettings.ClientId, appSettings.ClientSecret);
            var response = await tokenClient.RequestClientCredentialsAsync(appSettings.Scopes.First()).ConfigureAwait(false);

            if(!response.IsHttpError)
            {
                return response;
            }

            string exceptionMessage = Resource.AzureAccessTokenRetrievalFailure;
            throw new AzureActiveDirectoryException(exceptionMessage);
        }
    }
}