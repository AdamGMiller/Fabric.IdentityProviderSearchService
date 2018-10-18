using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Fabric.IdentityProviderSearchService.Models;
using System.Collections.Concurrent;
using IdentityModel.Client;
using System;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class MicrosoftGraphApi : IMicrosoftGraphApi
    {
        // String is tenant id and the Token response wrapper adds the expiry date time for easier cache invalidation.
        private static ConcurrentDictionary<string, TokenResponseWrapper> _tokensOfEachTenant;
        private static IDictionary<string, AzureClientApplicationSettings> _appSettings;
        private IAzureActiveDirectoryClientCredentialsService _azureActiveDirectoryClientCredentialsService;
        private IAppConfiguration _appConfiguration;

        public MicrosoftGraphApi(IAppConfiguration appConfiguration, IAzureActiveDirectoryClientCredentialsService settingsService)
        {
            _appSettings = AzureClientApplicationSettings.CreateDictionary(appConfiguration.AzureActiveDirectoryClientSettings);
            _azureActiveDirectoryClientCredentialsService = settingsService;
            _appConfiguration = appConfiguration;
            _tokensOfEachTenant = new ConcurrentDictionary<string, TokenResponseWrapper>();
        }

        public async Task<FabricGraphApiUser> GetUserAsync(string subjectId)
        {
            await GenerateAccessTokensAsync().ConfigureAwait(false);
            
            foreach (var tokenPair in _tokensOfEachTenant)
            {
                TokenResponseWrapper token;
                if (_tokensOfEachTenant.TryGetValue(tokenPair.Key, out token))
                {
                    var tenantId = tokenPair.Key;
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
            await GenerateAccessTokensAsync().ConfigureAwait(false);
            var searchTasks = new List<Task<IEnumerable<FabricGraphApiUser>>>();

            foreach (var tokenPair in _tokensOfEachTenant)
            {
                TokenResponseWrapper token;

                if (_tokensOfEachTenant.TryGetValue(tokenPair.Key, out token))
                {
                    var tenantId = tokenPair.Key;
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
            await GenerateAccessTokensAsync().ConfigureAwait(false);
            var searchTasks = new List<Task<IEnumerable<FabricGraphApiGroup>>>();

            foreach (var tokenPair in _tokensOfEachTenant)
            {
                TokenResponseWrapper token;

                if (_tokensOfEachTenant.TryGetValue(tokenPair.Key, out token))
                {
                    var tenantId = tokenPair.Key;
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

        private IGraphServiceClient GetNewClient(TokenResponseWrapper token)
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token.Response.AccessToken);

                return Task.FromResult(0);
            }));
        }

        private async Task GenerateAccessTokensAsync()
        {
            var tenantIds = _appSettings.Keys;

            foreach (var tenantId in tenantIds)
            {
                TokenResponseWrapper token;

                if (_tokensOfEachTenant.TryGetValue(tenantId, out token))
                {
                    if (token.ExpiryTime <= DateTime.Now)
                    {
                        await GetNewTokenAsync(tenantId).ConfigureAwait(false);
                    }
                }
                else
                {
                    await GetNewTokenAsync(tenantId).ConfigureAwait(false);
                }

            }
        }

        private async Task GetNewTokenAsync(string tenantId)
        {
            var response = await _azureActiveDirectoryClientCredentialsService.GetAzureAccessTokenAsync(tenantId).ConfigureAwait(false);
            var newToken = new TokenResponseWrapper() { ExpiryTime = DateTime.Now.AddSeconds(response.ExpiresIn), Response = response };
            _tokensOfEachTenant.AddOrUpdate(tenantId, newToken, (key, oldValue) => { return newToken; });
        }
    }
}