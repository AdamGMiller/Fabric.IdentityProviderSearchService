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

        private IEnumerable<AdPrincipal> Search(string searchText, PrincipalType principalType, List<AdPrincipal> principals)
        {
            var ctx = new PrincipalContext(ContextType.Domain, _domain);

            var queryFilter = GetQueryFilter(principalType, ctx);
            queryFilter.SamAccountName = $"{searchText}*";

            var searcher = new PrincipalSearcher(queryFilter);
            var principalResult = searcher.FindAll();

            foreach (var principal in principalResult)
            {
                var adPrincipal = new AdPrincipal {Name = principal.Name};

                if (principal is UserPrincipal)
                {
                    var userPrincipalResult = principal as UserPrincipal;

                    adPrincipal.FirstName = userPrincipalResult.GivenName;
                    adPrincipal.MiddleName = userPrincipalResult.MiddleName;
                    adPrincipal.LastName = userPrincipalResult.Surname;
                    adPrincipal.PrincipalType = PrincipalType.User;
                }
                else if (principal is GroupPrincipal)
                {
                    adPrincipal.PrincipalType = PrincipalType.Group;
                }

                principals.Add(adPrincipal);
            }

            return principals;
        }

        private Principal GetQueryFilter(PrincipalType principalType, PrincipalContext principalContext)
        {
            Principal principal;

            switch (principalType)
            {
                case PrincipalType.Group:
                    principal = new GroupPrincipal(principalContext);
                    break;
                case PrincipalType.User:
                    principal = new UserPrincipal(principalContext);
                    break;
                default:
                    principal = new ComputerPrincipal(principalContext);
                    break;
            }

            return principal;
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
                Name = ReadUserEntryProperty(userEntry.Properties["name"]),
                FirstName = ReadUserEntryProperty(userEntry.Properties["givenname"]),
                LastName = ReadUserEntryProperty(userEntry.Properties["sn"]),
                MiddleName = ReadUserEntryProperty(userEntry.Properties["middlename"]),
                PrincipalType = PrincipalType.User,                
                SubjectId = $"{_domain}\\{ReadUserEntryProperty(userEntry.Properties["samaccountname"])}"
            };
        }

        private AdPrincipal CreateGroupPrincipal(DirectoryEntry groupEntry)
        {
            return new AdPrincipal
            {
                Name = ReadUserEntryProperty(groupEntry.Properties["name"]),
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
    }
}