using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web.UI;
using Fabric.ActiveDirectory.Models;

namespace Fabric.ActiveDirectory.Services
{
    public class ActiveDirectoryProviderService : IExternalIdentityProviderService
    {
        private readonly string _domain;

        public ActiveDirectoryProviderService()
        {
            //TODO: dont hardcode this pass in the configuration in the constructor
            _domain = "hqcatalyst";
        }

        public IEnumerable<AdPrincipal> SearchPrincipals(string searchText, PrincipalType principalType)
        {
            var ldapQuery = BuildLdapQuery(searchText, principalType);

            var principals = FindPrincipalsWithDirectorySearcher(ldapQuery);

            return principals;
        }

        public AdPrincipal FindUserBySubjectId(string subjectId)
        {
            if (!subjectId.Contains(@"\"))
            {            
                return new AdPrincipal();
            }
            
            var subjectIdParts = subjectId.Split('\\');
            var domain = subjectIdParts[0];
            var accountName = subjectIdParts[subjectIdParts.Length - 1];

            var ctx = new PrincipalContext(ContextType.Domain, domain);
            var userPrincipalResult = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, accountName);

            if (userPrincipalResult == null)
            {
                return new AdPrincipal();
            }

            return new AdPrincipal
            {                
                FirstName = userPrincipalResult.GivenName,
                MiddleName = userPrincipalResult.MiddleName,
                LastName = userPrincipalResult.Surname,
                SubjectId = GetSubjectId(userPrincipalResult.SamAccountName),
                PrincipalType = PrincipalType.User
            };
        }

        private IEnumerable<AdPrincipal> FindPrincipalsWithDirectorySearcher(string ldapQuery)
        {
            var principals = new List<AdPrincipal>();
          
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

        private AdPrincipal CreateUserPrincipal(DirectoryEntry userEntry)
        {
            return new AdPrincipal
            {              
                FirstName = ReadUserEntryProperty(userEntry.Properties["givenname"]),
                LastName = ReadUserEntryProperty(userEntry.Properties["sn"]),
                MiddleName = ReadUserEntryProperty(userEntry.Properties["middlename"]),
                PrincipalType = PrincipalType.User,                
                SubjectId = GetSubjectId(ReadUserEntryProperty(userEntry.Properties["samaccountname"]))
            };
        }

        private AdPrincipal CreateGroupPrincipal(DirectoryEntry groupEntry)
        {
            return new AdPrincipal
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