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
            if (result == null)
            {
                return null;
            }

            var principal = CreateUserPrincipal(result);

            return principal;
        }

        public async Task<IEnumerable<IFabricPrincipal>> SearchPrincipalsAsync(
            string searchText,
            PrincipalType principalType,
            string searchType,
            string tenantId = null)
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
                    return await GetUserPrincipalsAsync(searchText, tenantId).ConfigureAwait(false);
                case PrincipalType.Group:
                    return await GetGroupPrincipalsAsync(searchText, tenantId).ConfigureAwait(false);
                default:
                    return await GetUserAndGroupPrincipalsAsync(searchText, tenantId).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<IFabricGroup>> SearchGroupsAsync(string searchText, string searchType, string tenantId = null)
        {
            var fabricGroups = new List<IFabricGroup>();

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

            var queryText = _azureQuery.QueryText(searchText, PrincipalType.Group);
            var fabricGraphApiGroups = await GetAllGroupsFromTenantsAsync(queryText, tenantId);
            foreach (var fabricGraphApiGroup in fabricGraphApiGroups)
            {
                fabricGroups.Add(CreateFabricGroup(fabricGraphApiGroup));
            }
            return fabricGroups;
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetUserAndGroupPrincipalsAsync(string searchText, string tenantId = null)
        {
            try
            {
                var userSearchTask = GetUserPrincipalsAsync(searchText, tenantId);
                var groupSearchTask = GetGroupPrincipalsAsync(searchText, tenantId);
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
                queryText = _azureQuery.QueryText(searchText, PrincipalType.User);
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

        private async Task<IEnumerable<IFabricPrincipal>> GetGroupPrincipalsAsync(string searchText, string tenantId = null)
        {
            string queryText = null;
            var principals = new List<IFabricPrincipal>();
            try
            {
                queryText = _azureQuery.QueryText(searchText, PrincipalType.Group);
                var groups = await GetAllGroupsFromTenantsAsync(queryText, tenantId).ConfigureAwait(false);

                if (groups != null)
                {
                    foreach (var result in groups)
                    {
                        principals.Add(CreateGroupPrincipal(result));
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

        private static IFabricPrincipal CreateUserPrincipal(FabricGraphApiUser userEntry)
        {
            var principal = new FabricPrincipal
            {
                UserPrincipal = userEntry.User.UserPrincipalName,
                TenantId = userEntry.TenantId,
                FirstName = userEntry.User.GivenName ?? userEntry.User.DisplayName,
                LastName = userEntry.User.Surname,
                MiddleName = string.Empty,   // this value does not exist in the graph api
                IdentityProvider = IdentityProviders.AzureActiveDirectory,
                PrincipalType = PrincipalType.User,
                SubjectId = userEntry.User.Id,
                IdentityProviderUserPrincipalName = userEntry.User.UserPrincipalName
            };

            principal.DisplayName = $"{principal.FirstName} {principal.LastName}";
            return principal;
        }

        private static IFabricPrincipal CreateGroupPrincipal(FabricGraphApiGroup groupEntry)
        {
            var result = new FabricPrincipal
            {
                SubjectId = groupEntry.Group.DisplayName,
                ExternalIdentifier = groupEntry.Group.Id,
                TenantId = groupEntry.TenantId,
                DisplayName = groupEntry.Group.DisplayName,
                IdentityProvider = IdentityProviders.AzureActiveDirectory,
                PrincipalType = PrincipalType.Group
            };

            return result;
        }

        private static IFabricGroup CreateFabricGroup(FabricGraphApiGroup groupEntry)
        {
            return new FabricGroup
            {
                ExternalIdentifier = groupEntry.Group.Id,
                TenantId = groupEntry.TenantId,
                GroupName = groupEntry.Group.DisplayName,
                IdentityProvider = IdentityProviders.AzureActiveDirectory,
                PrincipalType = PrincipalType.Group
            };
        }
    }
}