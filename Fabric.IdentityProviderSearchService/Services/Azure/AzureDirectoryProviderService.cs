using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Constants;
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

            var principal = CreateUserPrincipal<IFabricPrincipal>(result);

            return principal;
        }

        public async Task<IEnumerable<T>> SearchPrincipalsAsync<T>(string searchText, PrincipalType principalType, string searchType, string identityProvider = null, string tenantId = null)
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

            var filterQuery = _azureQuery.QueryText(searchText, principalType);

            switch (principalType)
            {
                case PrincipalType.User:
                    return (IEnumerable<T>) await GetUserPrincipalsAsync<IFabricPrincipal>(filterQuery, identityProvider, tenantId).ConfigureAwait(false);
                case PrincipalType.Group:
                    return (IEnumerable<T>) await GetGroupPrincipalsAsync<IFabricGroup>(filterQuery, identityProvider, tenantId).ConfigureAwait(false);
                default:
                    return (IEnumerable<T>) await GetUserAndGroupPrincipalsAsync(filterQuery, identityProvider, tenantId).ConfigureAwait(false);
            }
        }

        private async Task<IEnumerable<IFabricUserGroup>> GetUserAndGroupPrincipalsAsync(string searchText, string identityprovider = null, string tenantId = null)
        {
            var userSearchTask = GetUserPrincipalsAsync<IFabricUserGroup>(searchText, tenantId);
            var groupSearchTask = GetGroupPrincipalsAsync<IFabricUserGroup>(searchText, tenantId);
            var results = await Task.WhenAll(userSearchTask, groupSearchTask).ConfigureAwait(false);

            return results.SelectMany(result => result);
        }

        private async Task<IEnumerable<T>> GetUserPrincipalsAsync<T>(string searchText, string identityprovider = null, string tenantId = null)
        {
            var principals = new List<T>();
            var users = await GetAllUsersFromTenantsAsync(searchText, tenantId).ConfigureAwait(false);

            foreach(var result in users)
            {
                principals.Add(CreateUserPrincipal<T>(result));
            }

            return principals;
        }

        private async Task<IEnumerable<FabricGraphApiUser>> GetAllUsersFromTenantsAsync(string searchText, string identityprovider = null, string tenantId = null)
        {
           return await _graphApi.GetUserCollectionsAsync(searchText, tenantId).ConfigureAwait(false);
        }

        private async Task<IEnumerable<T>> GetGroupPrincipalsAsync<T>(string searchText, string identityprovider = null, string tenantId = null)
        {
            var principals = new List<T>();
            var groups = await GetAllGroupsFromTenantsAsync(searchText, tenantId).ConfigureAwait(false);

            foreach(var result in groups)
            {
                principals.Add(CreateGroupPrincipal<T>(result, identityprovider));
            }

            return principals;
        }

        private async Task<IEnumerable<FabricGraphApiGroup>> GetAllGroupsFromTenantsAsync(string searchText, string tenantId = null)
        {
            return await _graphApi.GetGroupCollectionsAsync(searchText, tenantId).ConfigureAwait(false);
        }

        private T CreateUserPrincipal<T>(FabricGraphApiUser userEntry)
        {
            Type modelType = typeof(T);
            if (modelType == typeof(IFabricPrincipal))
            {
                object result = new FabricPrincipal
                {
                    UserPrincipal = userEntry.User.UserPrincipalName,
                    TenantId = userEntry.TenantId,
                    FirstName = userEntry.User.GivenName ?? userEntry.User.DisplayName,
                    LastName = userEntry.User.Surname,
                    MiddleName = string.Empty,   // this value does not exist in the graph api
                    PrincipalType = PrincipalType.User,
                    SubjectId = userEntry.User.Id
                };
                return (T)result;
            }
            else if(modelType == typeof(IFabricUserGroup))
            {
                object result = new FabricUserGroup
                {
                    UserPrincipal = userEntry.User.UserPrincipalName,
                    TenantId = userEntry.TenantId,
                    FirstName = userEntry.User.GivenName ?? userEntry.User.DisplayName,
                    LastName = userEntry.User.Surname,
                    MiddleName = string.Empty,   // this value does not exist in the graph api
                    PrincipalType = PrincipalType.UserAndGroup,
                    SubjectId = userEntry.User.Id
                };
                return (T)result;
            }
            return default(T);
        }

        private T CreateGroupPrincipal<T>(FabricGraphApiGroup groupEntry, string identityprovider = null)
        {
            Type modelType = typeof(T);
            if (modelType == typeof(IFabricGroup))
            {
                object result = new FabricGroup
                {
                    GroupId = groupEntry.Group.Id,
                    TenantId = groupEntry.TenantId,
                    GroupName = groupEntry.Group.DisplayName,     // TODO: What should go here, the principal interface does not describe a graph group well
                    IdentityProvider = identityprovider,
                    PrincipalType = PrincipalType.Group
                };
                return (T)result;
            }
            else if (modelType == typeof(IFabricUserGroup))
            {
                object result = new FabricUserGroup
                {
                    GroupId = groupEntry.Group.Id,
                    TenantId = groupEntry.TenantId,
                    GroupName = groupEntry.Group.DisplayName,     // TODO: What should go here, the principal interface does not describe a graph group well
                    IdentityProvider = identityprovider,
                    PrincipalType = PrincipalType.UserAndGroup
                };
                return (T)result;
            }
            return default(T);
        }
    }
}