using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class ActiveDirectoryProxy : IActiveDirectoryProxy
    {
        public IEnumerable<IDirectoryEntry> SearchDirectory(string ldapQuery)
        {
            var directorySearcher = new DirectorySearcher(null, ldapQuery);
            var results = directorySearcher.FindAll();

            var searchResults = new List<IDirectoryEntry>();

            foreach (SearchResult searchResult in results)
            {
                searchResults.Add(new DirectoryEntryWrapper(searchResult.GetDirectoryEntry()));
            }

            return searchResults;
        }

        public UserPrincipal SearchForUser(string domain, string accountName)
        {
            var ctx = new PrincipalContext(ContextType.Domain, domain);
            return UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, accountName);
        }
    }
}