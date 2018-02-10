using System.Collections.Generic;
using System.Linq;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services;
using Moq;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public static class ActiveDirectoryProxyMockExtensions
    {
        public static Mock<IActiveDirectoryProxy> SetupActiveDirectoryProxy(this Mock<IActiveDirectoryProxy> mockAdProxy, IEnumerable<IDirectoryEntry> principals)
        {
            mockAdProxy.Setup(proxy => proxy.SearchDirectory(It.IsAny<string>()))
                .Returns((string ldapQuery) =>
                {
                    return principals.Where(p => p.SchemaClassName.Equals("user"));
                });

            return mockAdProxy;
        }
    }
}
