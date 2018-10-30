using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services.PrincipalQuery;
using System;

// TODO: General TODO, catch non-happy path requests, unauthorized, forbidden, 500. Return partial responses in these cases?
namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public class AzureDirectoryProviderService : IExternalIdentityProviderService
    {
        private readonly IMicrosoftGraphApi _graphApi;
        private IAzureQuery _azureQuery;

        public AzureDirectoryProviderService(IMicrosoftGraphApi graphApi)
        {
            _graphApi = graphApi;
        }
        
        public async Task<IFabricPrincipal> FindUserBySubjectIdAsync(string subjectId, string tenantId = null)
        {
            var result = await _graphApi.GetUserAsync(subjectId, tenantId).ConfigureAwait(false);
            if(result == null)
            {
                return null;
            }

            var principal = CreateUserPrincipal(result);

            return principal;
        }

        public async Task<IEnumerable<T>> SearchPrincipalsAsync<T>(string searchText, PrincipalType principalType, string searchType, string tenantId = null)
        {
            switch (searchType)
            {
                case SearchTypes.Wildcard:
                    _azureQuery = new AzureWildcardQuery();
                    break;
                case SearchTypes.Exact:
                    _azureQuery = new AzureExactMatchQuery();
                    break;
                default:
                    throw new Exception($"{searchType} is not a valid search type");
            }

            switch (principalType)
            {
                case PrincipalType.User:
                    return (IEnumerable<T>) await GetUserPrincipalsAsync(searchText, tenantId).ConfigureAwait(false);
                case PrincipalType.Group:
                    return await GetGroupPrincipalsAsync<T>(searchText, tenantId).ConfigureAwait(false);
                default:
                    return (IEnumerable<T>) await GetUserAndGroupPrincipalsAsync(searchText, tenantId).ConfigureAwait(false);
            }
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetUserAndGroupPrincipalsAsync(string searchText, string tenantId = null)
        {
            try
            {
                var userSearchTask = GetUserPrincipalsAsync(searchText, tenantId);

                var groupSearchTask = GetGroupPrincipalsAsync<IFabricPrincipal>(searchText, tenantId);

                var results = await Task.WhenAll(userSearchTask, groupSearchTask).ConfigureAwait(false);
                return results.SelectMany(result => result);
            }
            catch
            {
                throw new BadRequestException($"SearchText contained in {searchText} is not valid");
            }
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetUserPrincipalsAsync(string searchText, string tenantId = null)
        {
            string queryText = null;
            var principals = new List<IFabricPrincipal>();
            try
            {
                queryText = GetQueryText(searchText, PrincipalType.User);
                var users = await GetAllUsersFromTenantsAsync(queryText, tenantId).ConfigureAwait(false);

                foreach (var result in users)
                {
                    principals.Add(CreateUserPrincipal(result));
                }

                return principals;
            }
            catch
            {
                throw new BadRequestException($"SearchText contained in {queryText} is not valid");
            }
        }

        private async Task<IEnumerable<FabricGraphApiUser>> GetAllUsersFromTenantsAsync(string searchText, string tenantId = null)
        {
           return await _graphApi.GetUserCollectionsAsync(searchText, tenantId).ConfigureAwait(false);
        }

        private async Task<IEnumerable<T>> GetGroupPrincipalsAsync<T>(string searchText, string tenantId = null)
        {
            string queryText = null;
            var principals = new List<T>();
            try
            {
                queryText = GetQueryText(searchText, PrincipalType.Group);
                var groups = await GetAllGroupsFromTenantsAsync(queryText, tenantId).ConfigureAwait(false);

                if (groups != null)
                {
                    foreach (var result in groups)
                    {
                        principals.Add(CreateGroupPrincipal<T>(result));
                    }

                    return principals;
                }
                return null;
            }
            catch
            {
                throw new BadRequestException($"SearchText contained in {queryText} is not valid");
            }
        }

        private async Task<IEnumerable<FabricGraphApiGroup>> GetAllGroupsFromTenantsAsync(string searchText, string tenantId = null)
        {
            return await _graphApi.GetGroupCollectionsAsync(searchText, tenantId).ConfigureAwait(false);
        }

        private IFabricPrincipal CreateUserPrincipal(FabricGraphApiUser userEntry)
        {
            return new FabricPrincipal
            {
                UserPrincipal = userEntry.User.UserPrincipalName,
                TenantId = userEntry.TenantId,
                FirstName = userEntry.User.GivenName ?? userEntry.User.DisplayName,
                LastName = userEntry.User.Surname,
                MiddleName = string.Empty,   // this value does not exist in the graph api
                IdentityProvider = userEntry.IdentityProvider,
                PrincipalType = PrincipalType.User,
                SubjectId = userEntry.User.Id
            };
        }

        private T CreateGroupPrincipal<T>(FabricGraphApiGroup groupEntry)
        {
            Type modelType = typeof(T);
            if (modelType == typeof(IFabricGroup))
            {
                object result = new FabricGroup
                {
                    GroupId = groupEntry.Group.Id,
                    TenantId = groupEntry.TenantId,
                    GroupName = groupEntry.Group.DisplayName,     // TODO: What should go here, the principal interface does not describe a graph group well
                    IdentityProvider = groupEntry.IdentityProvider,
                    PrincipalType = PrincipalType.Group
                };
                return (T)result;
            }
            else if (modelType == typeof(IFabricPrincipal))
            {
                object result = new FabricPrincipal
                {
                    GroupId = groupEntry.Group.Id,
                    TenantId = groupEntry.TenantId,
                    GroupName = groupEntry.Group.DisplayName,     // TODO: What should go here, the principal interface does not describe a graph group well
                    IdentityProvider = groupEntry.IdentityProvider,
                    PrincipalType = PrincipalType.Group
                };
                return (T)result;
            }

            return default(T);
        }

        private string GetQueryText (string searchText, PrincipalType principalType)
        {
            return _azureQuery.QueryText(searchText, principalType);
        }
    }
}