using System.Collections.Generic;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class ActiveDirectoryProviderService : IExternalIdentityProviderService
    {
        private readonly IActiveDirectoryProxy _activeDirectoryProxy;
        private readonly string _domain;

        public ActiveDirectoryProviderService(IActiveDirectoryProxy activeDirectoryProxy, IAppConfiguration appConfig)
        {
            _activeDirectoryProxy = activeDirectoryProxy;
            _domain = appConfig.DomainName;
        }

        public IEnumerable<IFabricPrincipal> SearchPrincipals(string searchText, PrincipalType principalType)
        {
            var ldapQuery = BuildLdapQuery(searchText, principalType);

            var principals = FindPrincipalsWithDirectorySearcher(ldapQuery);

            return principals;
        }

        public IFabricPrincipal FindUserBySubjectId(string subjectId)
        {
            if (!subjectId.Contains(@"\"))
            {            
                return new FabricPrincipal();
            }
            
            var subjectIdParts = subjectId.Split('\\');
            var domain = subjectIdParts[0];
            var accountName = subjectIdParts[subjectIdParts.Length - 1];

            var userPrincipalResult = _activeDirectoryProxy.SearchForUser(domain, accountName);

            if (userPrincipalResult == null)
            {
                return new FabricPrincipal();
            }

            return new FabricPrincipal
            {                
                FirstName = userPrincipalResult.GivenName,
                MiddleName = userPrincipalResult.MiddleName,
                LastName = userPrincipalResult.Surname,
                SubjectId = GetSubjectId(userPrincipalResult.SamAccountName),
                PrincipalType = PrincipalType.User
            };
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
            return entryResult.SchemaClassName.Equals("user");
        }

        private IFabricPrincipal CreateUserPrincipal(IDirectoryEntry userEntry)
        {
            return new FabricPrincipal
            {              
                FirstName = userEntry.Properties["givenname"]?.ToString(),
                LastName = userEntry.Properties["sn"]?.ToString(),
                MiddleName = userEntry.Properties["middlename"]?.ToString(),
                PrincipalType = PrincipalType.User,                
                SubjectId = GetSubjectId(userEntry.Properties["samaccountname"]?.ToString())
            };
        }

        private IFabricPrincipal CreateGroupPrincipal(IDirectoryEntry groupEntry)
        {
            return new FabricPrincipal
            {
                SubjectId = GetSubjectId(groupEntry.Properties["name"]?.ToString()),
                PrincipalType = PrincipalType.Group
            };
        }

        private string BuildLdapQuery(string searchText, PrincipalType principalType)
        {
            var nameFilter = $"(|(sAMAccountName={searchText}*)(givenName={searchText}*)(sn={searchText}*))";

            switch (principalType)
            {
                case PrincipalType.User:
                    return $"(&(objectClass=user)(objectCategory=person){nameFilter})";
                case PrincipalType.Group:
                    return $"(&(objectCategory=group){nameFilter})";
                default:
                    return $"(&(|(&(objectClass=user)(objectCategory=person))(objectCategory=group)){nameFilter})";
            }
        }

        private string GetSubjectId(string sAmAccountName)
        {
            return $"{_domain}\\{sAmAccountName}";
        }
    }
}