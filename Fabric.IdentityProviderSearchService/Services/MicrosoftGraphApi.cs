using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Microsoft.Graph;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class MicrosoftGraphApi : IMicrosoftGraphApi
    {
        private static IDictionary<string, IGraphServiceClient> _graphClients = new ConcurrentDictionary<string, IGraphServiceClient>();

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
                _graphClients.Add(tenant, client);
            }
        }

        public async Task<FabricGraphApiUser> GetUserAsync(string subjectId)
        {
            await GenerateAccessTokensAsync().ConfigureAwait(false);
            
            foreach (var client in _graphClients)
            {
                var apiUser = await client.Value.Users[subjectId].Request().GetAsync().ConfigureAwait(false);
                if (apiUser != null)
                {
                    FabricGraphApiUser user = new FabricGraphApiUser(apiUser)
                    {
                        TenantId = client.Key
                    };
                    return user;
                }
            }

            return null;
        }

        public async Task<IEnumerable<FabricGraphApiUser>> GetUserCollectionsAsync(string filterQuery)
        {
            await GenerateAccessTokensAsync().ConfigureAwait(false);
            var searchTasks = new List<Task<IEnumerable<FabricGraphApiUser>>>();

            foreach (var client in _graphClients)
            {
                var tenantId = client.Key;

                var tempTask = Task.Run(async () => {
                    var taskResult = await client.Value.Users.Request().Filter(filterQuery).GetAsync();
                    return taskResult.Select(user => new FabricGraphApiUser(user as User)
                    {
                        TenantId = tenantId
                    });
                });

                searchTasks.Add(tempTask);
            }

            var results = await Task.WhenAll(searchTasks).ConfigureAwait(false);
            return results.SelectMany(result => result);
        }

        public async Task<IEnumerable<FabricGraphApiGroup>> GetGroupCollectionsAsync(string filterQuery)
        {
            await GenerateAccessTokensAsync().ConfigureAwait(false);
            var searchTasks = new List<Task<IEnumerable<FabricGraphApiGroup>>>();

            foreach (var client in _graphClients)
            {
                var tenantId = client.Key;
                var tempTask = Task.Run(async () => { 
                    var taskResult = await client.Value.Groups.Request().Filter(filterQuery).GetAsync();
                    return taskResult.Select(group => new FabricGraphApiGroup(group as Group)
                    {
                        TenantId = tenantId
                    });
                });

                searchTasks.Add(tempTask);
            }

            var results = await Task.WhenAll(searchTasks).ConfigureAwait(false);
            return results.SelectMany(result => result);
        }
    }
}