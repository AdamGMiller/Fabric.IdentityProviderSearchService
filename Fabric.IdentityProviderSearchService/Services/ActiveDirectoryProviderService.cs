using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class ActiveDirectoryProviderService : IExternalIdentityProviderService
    {        
        private readonly string _domain;

        public ActiveDirectoryProviderService(IAppConfiguration appConfig)
        {           
            _domain = appConfig.DomainName;
        }

        public IEnumerable<FabricPrincipal> SearchPrincipals(string searchText, PrincipalType principalType)
        {
            var ldapQuery = BuildLdapQuery(searchText, principalType);

            var principals = FindPrincipalsWithDirectorySearcher(ldapQuery);

            return principals;
        }

        public FabricPrincipal FindUserBySubjectId(string subjectId)
        {
            if (!subjectId.Contains(@"\"))
            {            
                return new FabricPrincipal();
            }
            
            var subjectIdParts = subjectId.Split('\\');
            var domain = subjectIdParts[0];
            var accountName = subjectIdParts[subjectIdParts.Length - 1];

            var ctx = new PrincipalContext(ContextType.Domain, domain);
            var userPrincipalResult = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, accountName);

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

        private IEnumerable<FabricPrincipal> FindPrincipalsWithDirectorySearcher(string ldapQuery)
        {
            var principals = new List<FabricPrincipal>();
          
            var directorySearcher = new DirectorySearcher(null, ldapQuery);

            var searchResults = directorySearcher.FindAll();

            foreach (SearchResult searchResult in searchResults)
            {
                var entryResult = searchResult.GetDirectoryEntry();

                principals.Add(IsDirectoryEntryAUser(entryResult)
                    ? CreateUserPrincipal(entryResult)
                    : CreateGroupPrincipal(entryResult));
            }

            return principals;
        }

        private bool IsDirectoryEntryAUser(DirectoryEntry entryResult)
        {            
            return entryResult.SchemaClassName.Equals("user");
        }

        private FabricPrincipal CreateUserPrincipal(DirectoryEntry userEntry)
        {
            return new FabricPrincipal
            {              
                FirstName = ReadUserEntryProperty(userEntry.Properties["givenname"]),
                LastName = ReadUserEntryProperty(userEntry.Properties["sn"]),
                MiddleName = ReadUserEntryProperty(userEntry.Properties["middlename"]),
                PrincipalType = PrincipalType.User,                
                SubjectId = GetSubjectId(ReadUserEntryProperty(userEntry.Properties["samaccountname"]))
            };
        }

        private FabricPrincipal CreateGroupPrincipal(DirectoryEntry groupEntry)
        {
            return new FabricPrincipal
            {
                SubjectId = GetSubjectId(ReadUserEntryProperty(groupEntry.Properties["name"])),
                PrincipalType = PrincipalType.Group
            };
        }

        private string ReadUserEntryProperty(PropertyValueCollection propertyValueCollection)
        {
            return propertyValueCollection.Value?.ToString() ?? string.Empty;
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