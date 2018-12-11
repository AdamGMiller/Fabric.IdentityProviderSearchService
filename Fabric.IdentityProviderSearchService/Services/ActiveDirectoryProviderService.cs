using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services.PrincipalQuery;
using Microsoft.Security.Application;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class ActiveDirectoryProviderService : IExternalIdentityProviderService
    {
        private readonly IActiveDirectoryProxy _activeDirectoryProxy;
        private readonly string _domain;
        private IActiveDirectoryQuery _activeDirectoryQuery;

        public ActiveDirectoryProviderService(IActiveDirectoryProxy activeDirectoryProxy, IAppConfiguration appConfig)
        {
            _activeDirectoryProxy = activeDirectoryProxy;
            _domain = appConfig.DomainName;
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
                    _activeDirectoryQuery = new ActiveDirectoryWildcardQuery();
                    break;
                case SearchTypes.Exact:
                    _activeDirectoryQuery = new ActiveDirectoryExactMatchQuery();
                    break;
                default:
                    throw new Exception($"{searchType} is not a valid search type");
            }
            var ldapQuery = _activeDirectoryQuery.QueryText(searchText, principalType);
            var principals = await Task.Run(() => FindPrincipalsWithDirectorySearcher(ldapQuery)).ConfigureAwait(false);
            return principals;
        }

        public async Task<IFabricPrincipal> FindUserBySubjectIdAsync(string subjectId, string tenantId = null)
        {
            if (!subjectId.Contains(@"\"))
            {            
                return null;
            }
            
            var subjectIdParts = subjectId.Split('\\');
            var domain = subjectIdParts[0];
            var accountName = subjectIdParts[subjectIdParts.Length - 1];

            var subject = await Task.Run(() => _activeDirectoryProxy.SearchForUser(Encoder.LdapFilterEncode(domain), Encoder.LdapFilterEncode(accountName))).ConfigureAwait(false);

            if (subject == null)
            {
                return null;
            }

            var principal = CreateUserPrincipal(subject);
            return principal;
        }

        public Task<IEnumerable<IFabricGroup>> SearchGroupsAsync(
            string searchText,
            string searchType,
            string tenantId = null)
        {
            var fabricGroups = new List<IFabricGroup>();

            switch (searchType)
            {
                case SearchTypes.Wildcard:
                    _activeDirectoryQuery = new ActiveDirectoryWildcardQuery();
                    break;
                case SearchTypes.Exact:
                    _activeDirectoryQuery = new ActiveDirectoryExactMatchQuery();
                    break;
                default:
                    throw new Exception($"{searchType} is not a valid search type");
            }

            var ldapQuery = _activeDirectoryQuery.QueryText(searchText, PrincipalType.Group);
            var searchResults = _activeDirectoryProxy.SearchDirectory(ldapQuery);

            foreach (var searchResult in searchResults)
            {
                if (!IsDirectoryEntryAUser(searchResult))
                {
                    fabricGroups.Add(CreateFabricGroup(searchResult));
                }
            }
            return Task.FromResult(fabricGroups.AsEnumerable());
        }

        private IEnumerable<IFabricPrincipal> FindPrincipalsWithDirectorySearcher(string ldapQuery)
        {
            var principals = new List<IFabricPrincipal>();

            var searchResults = _activeDirectoryProxy.SearchDirectory(ldapQuery);

            foreach (var searchResult in searchResults)
            {
                principals.Add(IsDirectoryEntryAUser(searchResult)
                    ? CreateUserPrincipal(searchResult)
                    : CreateGroupPrincipal(searchResult));
            }
            return principals;
        }

        private static bool IsDirectoryEntryAUser(IDirectoryEntry entryResult)
        {
            // TODO: Add to constants file
            return entryResult.SchemaClassName.Equals("user");
        }

        private IFabricPrincipal CreateUserPrincipal(IDirectoryEntry userEntry)
        {
            var subjectId = GetSubjectId(userEntry.SamAccountName);
            var principal = new FabricPrincipal
            {
                SubjectId = subjectId,
                DisplayName = $"{userEntry.FirstName} {userEntry.LastName}",
                FirstName = userEntry.FirstName,
                LastName = userEntry.LastName,
                MiddleName = userEntry.MiddleName,
                IdentityProvider = IdentityProviders.ActiveDirectory,
                PrincipalType = PrincipalType.User,
                IdentityProviderUserPrincipalName = subjectId
            };

            principal.DisplayName = $"{principal.FirstName} {principal.LastName}";
            return principal;
        }
        private static IFabricPrincipal CreateUserPrincipal(IFabricPrincipal userEntry)
        {
            var principal = new FabricPrincipal
            {
                UserPrincipal = userEntry.UserPrincipal,
                TenantId = userEntry.TenantId,
                FirstName = userEntry.FirstName,
                LastName = userEntry.LastName,
                MiddleName = userEntry.MiddleName, 
                IdentityProvider = IdentityProviders.ActiveDirectory,
                PrincipalType = PrincipalType.User,
                SubjectId = userEntry.SubjectId,
                IdentityProviderUserPrincipalName = userEntry.SubjectId
            };

            principal.DisplayName = $"{principal.FirstName} {principal.LastName}";
            return principal;
        }

        private IFabricPrincipal CreateGroupPrincipal(IDirectoryEntry groupEntry)
        {
            var subjectId = GetSubjectId(groupEntry.Name);
            return new FabricPrincipal
            {
                SubjectId = subjectId,
                DisplayName = subjectId,
                IdentityProvider = IdentityProviders.ActiveDirectory,
                PrincipalType = PrincipalType.Group
            };
        }

        private IFabricGroup CreateFabricGroup(IDirectoryEntry groupEntry)
        {
            var subjectId = GetSubjectId(groupEntry.Name);
            return new FabricGroup
            {
                GroupName = subjectId,
                IdentityProvider = IdentityProviders.ActiveDirectory,
                PrincipalType = PrincipalType.Group
            };
        }

        private string GetSubjectId(string sAmAccountName)
        {
            return $"{_domain}\\{sAmAccountName}";
        }
    }
}