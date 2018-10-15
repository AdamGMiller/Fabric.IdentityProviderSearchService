using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Microsoft.Graph;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class AzureDirectoryProviderService : IExternalIdentityProviderService
    {
        private IAppConfiguration _appConfiguration;
        private IAzureActiveDirectoryClientCredentialsService _azureActiveDirectoryClientCredentialsService;
        private string[] _tenantIds;
        private ICollection<IGraphServiceClient> _clients;

        public AzureDirectoryProviderService(IAppConfiguration appConfiguration, IAzureActiveDirectoryClientCredentialsService azureActiveDirectoryClientCredentialsService)
        {
            _appConfiguration = appConfiguration;
            _azureActiveDirectoryClientCredentialsService = azureActiveDirectoryClientCredentialsService;
            _tenantIds = _appConfiguration.AzureActiveDirectoryClientSettings.IssuerWhiteList;

            GenerateAccessTokensForTenantsAsync().Wait();
        }

        private async Task GenerateAccessTokensForTenantsAsync()
        {
            _clients = new List<IGraphServiceClient>();
            foreach(var tenant in _tenantIds)
            {
                var response = await _azureActiveDirectoryClientCredentialsService.GetAzureAccessTokenAsync(tenant).ConfigureAwait(false);

                var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => {
                    requestMessage
                        .Headers
                        .Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", response.access_token);

                    return Task.FromResult(0);
                }));
                _clients.Add(client);
            }
        }

        public IFabricPrincipal FindUserBySubjectId(string subjectId)
        {
            var result = GetUserAsync(subjectId).Result;
            var principal = CreateUserPrincipal(result);

            return principal;
        }

        public IEnumerable<IFabricPrincipal> SearchPrincipals(string searchText, PrincipalType principalType)
        {
            switch(principalType)
            {
                case PrincipalType.User:
                    return GetUserPrincipalsAsync(searchText).Result;
                case PrincipalType.Group:
                    return GetGroupPrincipalsAsync(searchText).Result;
                default:
                    return GetUserAndGroupPrincipalsAsync(searchText).Result;
            }
        }

        private async Task<User> GetUserAsync(string subjectId)
        {
            foreach(var client in _clients)
            {
                var user = await client.Users[subjectId].Request().GetAsync();
                if(user != null)
                {
                    return user;
                }
            }

            // TODO: throw not found?
            return new User(); 
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetUserAndGroupPrincipalsAsync(string searchText)
        {
            var results = await Task.WhenAll(
                Task.Run(() => GetUserPrincipalsAsync(searchText)),
                Task.Run(() => GetGroupPrincipalsAsync(searchText))
            ).ConfigureAwait(false);
            return results.SelectMany(result => result);
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetUserPrincipalsAsync(string searchText)
        {
            var principals = new List<IFabricPrincipal>();
            var users = await GetAllUsersFromTenantsAsync(searchText);

            foreach(var result in users)
            {
                principals.Add(CreateUserPrincipal(result));
            }

            return principals;
        }

        private async Task<IEnumerable<User>> GetAllUsersFromTenantsAsync(string searchText)
        {
            var filterQuery =
                $"startswith(DisplayName, '{searchText}') or startswith(GivenName, '{searchText}') or startswith(UserPrincipalName, '{searchText}')"; 
            // or startswith(PreferredName, '{searchText}') or startswith(UserPrincipalName, '{searchText}')";
            // TODO: why does preferredname not filter

            var users = new List<User>();
            foreach (var client in _clients)
            {
                 users.AddRange((await client.Users.Request().Filter(filterQuery).GetAsync()));
            }

            return users;
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetGroupPrincipalsAsync(string searchText)
        {
            var principals = new List<IFabricPrincipal>();
            var groups = await GetAllGroupsFromTenantsAsync(searchText).ConfigureAwait(false);

            foreach(var result in groups)
            {
                principals.Add(CreateGroupPrincipal(result));
            }

            return principals;
        }

        private async Task<IEnumerable<Group>> GetAllGroupsFromTenantsAsync(string searchText)
        {
            var filterQuery = $"startswith(DisplayName, '{searchText}')";
            var groups = new List<Group>();

            foreach (var client in _clients)
            {
                groups.AddRange(await client.Groups.Request().Filter(filterQuery).GetAsync());
            }

            return groups;
        }

        private IFabricPrincipal CreateUserPrincipal(User userEntry)
        {
            return new FabricPrincipal
            {
                FirstName = userEntry.GivenName ?? userEntry.DisplayName,
                LastName = userEntry.Surname,
                MiddleName = string.Empty,   // don't think this has a value in graph api/azure ad
                PrincipalType = PrincipalType.User,
                SubjectId = userEntry.Id
            };
        }

        private IFabricPrincipal CreateGroupPrincipal(Group groupEntry)
        {
            return new FabricPrincipal
            {
                SubjectId = groupEntry.Id,
                FirstName = groupEntry.DisplayName,     // TODO: What should go here, the interface doesn't describe a group well
                PrincipalType = PrincipalType.Group
            };
        }
    }
}