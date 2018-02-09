using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class ActiveDirectoryServiceProxy : IActiveDirectoryServiceProxy
    {
        public SearchResultCollection SearchDirectory(string ldapQuery)
        {
            var directorySearcher = new DirectorySearcher(null, ldapQuery);
            return directorySearcher.FindAll();
        }

        public UserPrincipal SearchForUser(string domain, string accountName)
        {
            var ctx = new PrincipalContext(ContextType.Domain, domain);
            return UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, accountName);
        }
    }
}