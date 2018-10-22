using Fabric.IdentityProviderSearchService.Configuration;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Fabric.IdentityProviderSearchService.Models;
using IdentityModel.Client;

namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public class MicrosoftGraphApi : IMicrosoftGraphApi
    {
        private static IDictionary<string, AzureClientApplicationSettings> _appSettings;
        private IAzureActiveDirectoryClientCredentialsService _azureActiveDirectoryClientCredentialsService;
        private IAppConfiguration _appConfiguration;

        public MicrosoftGraphApi(IAppConfiguration appConfiguration, IAzureActiveDirectoryClientCredentialsService settingsService)
        {
            _appSettings = AzureClientApplicationSettings.CreateDictionary(appConfiguration.AzureActiveDirectoryClientSettings);
            _azureActiveDirectoryClientCredentialsService = settingsService;
            _appConfiguration = appConfiguration;
        }

        public async Task<FabricGraphApiUser> GetUserAsync(string subjectId)
        {
            foreach (var tenantId in _appSettings.Keys)
            {
                var token = await _azureActiveDirectoryClientCredentialsService.GetAzureAccessTokenAsync(tenantId);
                if (token != null)
                {
                    var client = GetNewClient(token);
                    var apiUser = await client.Users[subjectId].Request().GetAsync().ConfigureAwait(false);
                    if (apiUser != null)
                    {
                        FabricGraphApiUser user = new FabricGraphApiUser(apiUser)
                        {
                            TenantId = tenantId
                        };
                        return user;
                    }
                }
            }

            return null;
        }

        public async Task<IEnumerable<FabricGraphApiUser>> GetUserCollectionsAsync(string filterQuery)
        {
            var searchTasks = new List<Task<IEnumerable<FabricGraphApiUser>>>();

            foreach (var tenantId in _appSettings.Keys)
            {
                var token = await _azureActiveDirectoryClientCredentialsService.GetAzureAccessTokenAsync(tenantId);
                if (token != null)
                {
                    var client = GetNewClient(token);
                    var tempTask = Task.Run(async () =>
                    {
                        // how does this handle a client not working (i.e. exceptions?)
                        var taskResult = await client.Users.Request().Filter(filterQuery).GetAsync();
                        return taskResult.Select(user => new FabricGraphApiUser(user as User)
                        {
                            TenantId = tenantId
                        });
                    });

                    searchTasks.Add(tempTask);
                }
            }

            var results = await Task.WhenAll(searchTasks).ConfigureAwait(false);
            return results.SelectMany(result => result);
        }

        public async Task<IEnumerable<FabricGraphApiGroup>> GetGroupCollectionsAsync(string filterQuery)
        {
            var searchTasks = new List<Task<IEnumerable<FabricGraphApiGroup>>>();

            foreach (var tenantId in _appSettings.Keys)
            {
                var token = await _azureActiveDirectoryClientCredentialsService.GetAzureAccessTokenAsync(tenantId);
                if (token != null)
                {
                    var client = GetNewClient(token);
                    var tempTask = Task.Run(async () =>
                    {
                        var taskResult = await client.Groups.Request().Filter(filterQuery).GetAsync();
                        return taskResult.Select(group => new FabricGraphApiGroup(group as Group)
                        {
                            TenantId = tenantId
                        });
                    });

                    searchTasks.Add(tempTask);
                }
            }

            var results = await Task.WhenAll(searchTasks).ConfigureAwait(false);
            return results.SelectMany(result => result);
        }

        private IGraphServiceClient GetNewClient(TokenResponse token)
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token.AccessToken);

                return Task.FromResult(0);
            }));
        }
    }
}