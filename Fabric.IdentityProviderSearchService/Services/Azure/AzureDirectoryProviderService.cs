﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Models;
using Microsoft.Graph;

// TODO: General TODO, catch non-happy path requests, unauthorized, forbidden, 500. Return partial responses in these cases?
namespace Fabric.IdentityProviderSearchService.Services
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

            var principal = CreateUserPrincipal(result);

            return principal;
        }

        public async Task<IEnumerable<IFabricPrincipal>> SearchPrincipalsAsync(string searchText, PrincipalType principalType)
        {
            switch(principalType)
            {
                case PrincipalType.User:
                    return await GetUserPrincipalsAsync(searchText);
                case PrincipalType.Group:
                    return await GetGroupPrincipalsAsync(searchText);
                default:
                    return await GetUserAndGroupPrincipalsAsync(searchText);
            }
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
            var users = await GetAllUsersFromTenantsAsync(searchText).ConfigureAwait(false);

            foreach(var result in users)
            {
                principals.Add(CreateUserPrincipal(result));
            }

            return principals;
        }

        private async Task<IEnumerable<FabricGraphApiUser>> GetAllUsersFromTenantsAsync(string searchText)
        {
            var filterQuery =
                $"startswith(DisplayName, '{searchText}') or startswith(GivenName, '{searchText}') or startswith(UserPrincipalName, '{searchText}') or startswith(Surname, '{searchText}')";

            return await _graphApi.GetUserCollectionsAsync(filterQuery).ConfigureAwait(false);
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

        private async Task<IEnumerable<FabricGraphApiGroup>> GetAllGroupsFromTenantsAsync(string searchText)
        {
            var filterQuery = $"startswith(DisplayName, '{searchText}')";

            return await _graphApi.GetGroupCollectionsAsync(filterQuery).ConfigureAwait(false);
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
                PrincipalType = PrincipalType.User,
                SubjectId = userEntry.User.Id
            };
        }

        private IFabricPrincipal CreateGroupPrincipal(FabricGraphApiGroup groupEntry)
        {
            return new FabricPrincipal
            {
                SubjectId = groupEntry.Group.Id,
                TenantId = groupEntry.TenantId,
                FirstName = groupEntry.Group.DisplayName,     // TODO: What should go here, the principal interface does not describe a graph group well
                PrincipalType = PrincipalType.Group
            };
        }
    }
}