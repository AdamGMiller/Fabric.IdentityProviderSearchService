using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IActiveDirectoryServiceProxy
    {
        SearchResultCollection SearchDirectory(string ldapQuery);
        UserPrincipal SearchForUser(string domain, string accountName);

    }
}
