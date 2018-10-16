using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class MicrosoftGraphApi : IMicrosoftGraphApi
    {
        //IAzureActiveDirectoryClientCredentialsService azureActiveDirectoryClientCredentialsService
        private static ICollection<IGraphServiceClient> _graphClients;
        private IAzureActiveDirectoryClientCredentialsService _azureActiveDirectoryClientCredentialsService;
        private IAppConfiguration _appConfiguration;

        public MicrosoftGraphApi(IAppConfiguration appConfiguration, IAzureActiveDirectoryClientCredentialsService settingsService)
        {
            _azureActiveDirectoryClientCredentialsService = settingsService;
            _appConfiguration = appConfiguration;
        }

        private async Task GenerateAccessTokensAsync()
        {
            // TODO: add code to do an if block
            // if the timeout is up, then update code
            // that way the client can call this all the time
            // without figuring out the expiration stuff.
            var tenantIds = _appConfiguration.AzureActiveDirectoryClientSettings.IssuerWhiteList;
            _graphClients = new List<IGraphServiceClient>();
            foreach (var tenant in tenantIds)
            {
                var response = await _azureActiveDirectoryClientCredentialsService.GetAzureAccessTokenAsync(tenant).ConfigureAwait(false);

                var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
                {
                    requestMessage
                        .Headers
                        .Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", response.AccessToken);

                    return Task.FromResult(0);
                }));
                _graphClients.Add(client);
            }
        }

        public async Task<User> GetUserAsync(string subjectId)
        {
            await GenerateAccessTokensAsync();
            
            foreach (var client in _graphClients)
            {
                var user = await client.Users[subjectId].Request().GetAsync();
                if (user != null)
                {
                    return user;
                }
            }

            return null;
        }

        public async Task<IEnumerable<User>> GetUserCollectionsAsync(string filterQuery)
        {
            await GenerateAccessTokensAsync();
            var searchTasks = new List<Task<IGraphServiceUsersCollectionPage>>();

            foreach (var client in _graphClients)
            {
                searchTasks.Add(client.Users.Request().Filter(filterQuery).GetAsync());
            }

            return await Task.WhenAll(searchTasks).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Group>> GetGroupCollectionsAsync(string filterQuery)
        {
            await GenerateAccessTokensAsync();
            var searchTasks = new List<Task<IGraphServiceGroupsCollectionPage>>();

            foreach (var client in _graphClients)
            {
                searchTasks.Add(client.Groups.Request().Filter(filterQuery).GetAsync());
            }

            return await Task.WhenAll(searchTasks).ConfigureAwait(false);
        }
    }
}