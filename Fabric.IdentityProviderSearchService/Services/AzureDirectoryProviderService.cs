using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Microsoft.Graph;

// TODO: General TODO, catch non-happy path requests, unauthorized, forbidden, 500. Return partial responses in these cases?
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

            // TODO: cache, only renew once token expires
            foreach(var tenant in _tenantIds)
            {
                var response = await _azureActiveDirectoryClientCredentialsService.GetAzureAccessTokenAsync(tenant).ConfigureAwait(false);

                var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => {
                    requestMessage
                        .Headers
                        .Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", response.AccessToken);

                    return Task.FromResult(0);
                }));
                _clients.Add(client);
            }
        }

        public IFabricPrincipal FindUserBySubjectId(string subjectId)
        {
            var result = GetUserAsync(subjectId).Result;
            if(result == null)
            {
                return null;
            }

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

            return null; 
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetUserAndGroupPrincipalsAsync(string searchText)
        {
            var userSearchTask = GetUserPrincipalsAsync(searchText);
            var groupSearchTask = GetGroupPrincipalsAsync(searchText);
            var results = await Task.WhenAll(userSearchTask, groupSearchTask).ConfigureAwait(false);

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

            var searchTasks = new List<Task<IGraphServiceUsersCollectionPage>>();

            foreach (var client in _clients)
            {
                searchTasks.Add(client.Users.Request().Filter(filterQuery).GetAsync());
            }
            var results = await Task.WhenAll(searchTasks).ConfigureAwait(false);
            
            return results.SelectMany(result => result);
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

            var searchTasks = new List<Task<IGraphServiceGroupsCollectionPage>>();

            foreach (var client in _clients)
            {
                searchTasks.Add(client.Groups.Request().Filter(filterQuery).GetAsync());
            }

            var results = await Task.WhenAll(searchTasks).ConfigureAwait(false);

            return results.SelectMany(result => result);
        }

        private IFabricPrincipal CreateUserPrincipal(User userEntry)
        {
            return new FabricPrincipal
            {
                UserPrincipal = userEntry.UserPrincipalName,
                FirstName = userEntry.GivenName ?? userEntry.DisplayName,
                LastName = userEntry.Surname,
                MiddleName = string.Empty,   // this value does not exist in the graph api
                PrincipalType = PrincipalType.User,
                SubjectId = userEntry.Id
            };
        }

        private IFabricPrincipal CreateGroupPrincipal(Group groupEntry)
        {
            return new FabricPrincipal
            {
                SubjectId = groupEntry.Id,
                FirstName = groupEntry.DisplayName,     // TODO: What should go here, the principal interface does not describe a graph group well
                PrincipalType = PrincipalType.Group
            };
        }
    }
}