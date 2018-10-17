using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Exceptions;
using IdentityModel.Client;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public class AzureActiveDirectoryClientCredentialsService : IAzureActiveDirectoryClientCredentialsService
    {
        private AzureActiveDirectoryClientSettings azureClientSettings;
        private HttpClient client;

        public AzureActiveDirectoryClientCredentialsService(AzureActiveDirectoryClientSettings azureClientSettings, HttpClient client)
        {
            this.azureClientSettings = azureClientSettings;
            this.client = client;
        }

        public async Task<TokenResponse> GetAzureAccessTokenAsync(string tenantId)
        {
            // TODO: Move "oauth2/v2.0/token" into some config?
            TokenClient tokenClient = new TokenClient($"{azureClientSettings.Authority.TrimEnd('/')}/{tenantId}/oauth2/v2.0/token", azureClientSettings.ClientId, azureClientSettings.ClientSecret);
            var response = await tokenClient.RequestClientCredentialsAsync(azureClientSettings.Scopes.First()).ConfigureAwait(false);

            if(!response.IsHttpError)
            {
                return response;
            }

            string exceptionMessage = Resource.AzureAccessTokenRetrievalFailure;
            throw new AzureActiveDirectoryException(exceptionMessage);
        }
    }
}