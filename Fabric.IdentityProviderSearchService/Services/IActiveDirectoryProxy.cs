using System.Collections.Generic;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IActiveDirectoryProxy
    {
        IEnumerable<IDirectoryEntry> SearchDirectory(string ldapQuery);
        IFabricPrincipal SearchForUser(string domain, string accountName);
    }
}
