using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Services;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public class MockActiveDirectoryServiceProxy : IActiveDirectoryServiceProxy
    {
        public SearchResultCollection SearchDirectory(string ldapQuery)
        {
            throw new NotImplementedException();
        }

        public UserPrincipal SearchForUser(string domain, string accountName)
        {
            throw new NotImplementedException();
        }
    }
}
