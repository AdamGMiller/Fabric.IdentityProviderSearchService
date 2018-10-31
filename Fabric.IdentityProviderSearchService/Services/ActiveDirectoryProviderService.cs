using System.Collections.Generic;
using System;
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
        public async Task<IEnumerable<T>> SearchPrincipalsAsync<T>(string searchText, PrincipalType principalType, string searchType, string tenantId = null)
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
            return (IEnumerable<T>)principals;
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

            return await Task.Run(() => _activeDirectoryProxy.SearchForUser(Encoder.LdapFilterEncode(domain), Encoder.LdapFilterEncode(accountName))).ConfigureAwait(false);
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

        private bool IsDirectoryEntryAUser(IDirectoryEntry entryResult)
        {
            // TODO: Add to constants file
            return entryResult.SchemaClassName.Equals("user");
        }

        private IFabricPrincipal CreateUserPrincipal(IDirectoryEntry userEntry)
        {
            var subjectId = GetSubjectId(userEntry.SamAccountName);
            var principal = new FabricPrincipal
            {
                FirstName = userEntry.FirstName,
                LastName = userEntry.LastName,
                MiddleName = userEntry.MiddleName,
                PrincipalType = PrincipalType.User,
                SubjectId = subjectId,
                UniqueId = $"{subjectId}-{PrincipalType.User}"
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
                UniqueId = $"{subjectId}-{PrincipalType.Group}",
                DisplayName = subjectId,
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