using System;
using System.Collections.Generic;
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

        public async Task<IEnumerable<IFabricPrincipal>> SearchPrincipalsAsync(string searchText, PrincipalType principalType, string searchType)
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
            var principals = await Task.Run(() => FindPrincipalsWithDirectorySearcher(ldapQuery));
            return principals;
        }

        public async Task<IFabricPrincipal> FindUserBySubjectIdAsync(string subjectId)
        {
            if (!subjectId.Contains(@"\"))
            {            
                return null;
            }
            
            var subjectIdParts = subjectId.Split('\\');
            var domain = subjectIdParts[0];
            var accountName = subjectIdParts[subjectIdParts.Length - 1];

            return await Task.Run(() => _activeDirectoryProxy.SearchForUser(Encoder.LdapFilterEncode(domain), Encoder.LdapFilterEncode(accountName)));
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
            return new FabricPrincipal
            {              
                FirstName = userEntry.FirstName,
                LastName = userEntry.LastName,
                MiddleName = userEntry.MiddleName,
                PrincipalType = PrincipalType.User,                
                SubjectId = GetSubjectId(userEntry.SamAccountName)
            };
        }

        private IFabricPrincipal CreateGroupPrincipal(IDirectoryEntry groupEntry)
        {
            return new FabricPrincipal
            {
                SubjectId = GetSubjectId(groupEntry.Name),
                PrincipalType = PrincipalType.Group
            };
        }

        private string GetSubjectId(string sAmAccountName)
        {
            return $"{_domain}\\{sAmAccountName}";
        }
    }
}