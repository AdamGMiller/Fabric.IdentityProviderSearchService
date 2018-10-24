using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Exceptions;
using System;

// TODO: General TODO, catch non-happy path requests, unauthorized, forbidden, 500. Return partial responses in these cases?
namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public class AzureDirectoryProviderService : IExternalIdentityProviderService
    {
        IMicrosoftGraphApi _graphApi;

        public AzureDirectoryProviderService(IMicrosoftGraphApi graphApi)
        {
            _graphApi = graphApi;
        }
        
        public async Task<IFabricPrincipal> FindUserBySubjectIdAsync(string subjectId)
        {
            var result = await _graphApi.GetUserAsync(subjectId);
            if(result == null)
            {
                return null;
            }

            var principal = CreateUserPrincipal<IFabricPrincipal>(result);

            return principal;
        }

        public async Task<IEnumerable<T>> SearchPrincipalsAsync<T>(string searchText, PrincipalType principalType)
        {
            switch(principalType)
            {
                case PrincipalType.User:
                    return (IEnumerable<T>) await GetUserPrincipalsAsync<IFabricPrincipal>(searchText);
                case PrincipalType.Group:
                    return (IEnumerable<T>) await GetGroupPrincipalsAsync<IFabricGroup>(searchText);
                default:
                    return (IEnumerable<T>) await GetUserAndGroupPrincipalsAsync(searchText);
            }
        }

        private async Task<IEnumerable<IFabricUserGroup>> GetUserAndGroupPrincipalsAsync(string searchText)
        {
            var userSearchTask = GetUserPrincipalsAsync<IFabricUserGroup>(searchText);
            var groupSearchTask = GetGroupPrincipalsAsync<IFabricUserGroup>(searchText);
            var results = await Task.WhenAll(userSearchTask, groupSearchTask).ConfigureAwait(false);

            return results.SelectMany(result => result);
        }

        private async Task<IEnumerable<T>> GetUserPrincipalsAsync<T>(string searchText)
        {
            var principals = new List<T>();
            var users = await GetAllUsersFromTenantsAsync(searchText).ConfigureAwait(false);

            foreach(var result in users)
            {
                principals.Add(CreateUserPrincipal<T>(result));
            }

            return principals;
        }

        private async Task<IEnumerable<FabricGraphApiUser>> GetAllUsersFromTenantsAsync(string searchText)
        {
            var filterQuery =
                $"startswith(DisplayName, '{searchText}') or startswith(GivenName, '{searchText}') or startswith(UserPrincipalName, '{searchText}') or startswith(Surname, '{searchText}')";

            return await _graphApi.GetUserCollectionsAsync(filterQuery).ConfigureAwait(false);
        }

        private async Task<IEnumerable<T>> GetGroupPrincipalsAsync<T>(string searchText)
        {
            var principals = new List<T>();
            var groups = await GetAllGroupsFromTenantsAsync(searchText).ConfigureAwait(false);

            foreach(var result in groups)
            {
                principals.Add(CreateGroupPrincipal<T>(result));
            }

            return principals;
        }

        private async Task<IEnumerable<FabricGraphApiGroup>> GetAllGroupsFromTenantsAsync(string searchText)
        {
            var filterQuery = $"startswith(DisplayName, '{searchText}')";
            return await _graphApi.GetGroupCollectionsAsync(filterQuery).ConfigureAwait(false);
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

        private T CreateGroupPrincipal<T>(FabricGraphApiGroup groupEntry)
        {
            Type modelType = typeof(T);
            if (modelType == typeof(IFabricGroup))
            {
                object result = new FabricGroup
                {
                    GroupId = groupEntry.Group.Id,
                    TenantId = groupEntry.TenantId,
                    GroupFirstName = groupEntry.Group.DisplayName,     // TODO: What should go here, the principal interface does not describe a graph group well
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
                    GroupFirstName = groupEntry.Group.DisplayName,     // TODO: What should go here, the principal interface does not describe a graph group well
                    PrincipalType = PrincipalType.UserAndGroup
                };
                return (T)result;
            }
            return default(T);
        }
    }
}