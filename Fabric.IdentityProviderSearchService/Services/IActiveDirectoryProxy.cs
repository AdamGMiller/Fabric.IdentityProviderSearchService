using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IActiveDirectoryProxy
    {
        IEnumerable<IDirectoryEntry> SearchDirectory(string ldapQuery);
        UserPrincipal SearchForUser(string domain, string accountName);
    }
}
