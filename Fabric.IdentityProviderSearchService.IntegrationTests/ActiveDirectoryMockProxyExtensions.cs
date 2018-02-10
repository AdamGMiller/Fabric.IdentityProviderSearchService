﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services;
using Moq;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public static class ActiveDirectoryProxyMockExtensions
    {
        private static string _userAndGroupSearchForPat =
            "(&(|(&(objectClass=user)(objectCategory=person))(objectCategory=group))(|(sAMAccountName=pat*)(givenName=pat*)(sn=pat*)))";

        private static string _groupSearchForPat =
            "(&(objectCategory=group)(|(sAMAccountName=pat*)(givenName=pat*)(sn=pat*)))";

        private static string _userSearchForPat =
            "(&(objectClass=user)(objectCategory=person)(|(sAMAccountName=pat*)(givenName=pat*)(sn=pat*)))";

        public static Mock<IActiveDirectoryProxy> SetupActiveDirectoryProxy(this Mock<IActiveDirectoryProxy> mockAdProxy, IEnumerable<IDirectoryEntry> principals)
        {
            mockAdProxy.Setup(proxy => proxy.SearchDirectory(It.Is<string>(s => s == _userAndGroupSearchForPat)))
                .Returns((string ldapQuery) =>
                {
                    return principals.Where(p =>
                        ((Dictionary<string, string>) p.Properties).Any(pv => pv.Value.ToString().StartsWith("pat")));
                });

            mockAdProxy.Setup(proxy => proxy.SearchDirectory(It.Is<string>(s => s == _groupSearchForPat)))
                .Returns((string ldapQuery) =>
                {
                    return principals.Where(p => p.SchemaClassName.Equals("group") &&
                        ((Dictionary<string, string>)p.Properties).Any(pv => pv.Value.ToString().StartsWith("pat")));
                });

            mockAdProxy.Setup(proxy => proxy.SearchDirectory(It.Is<string>(s => s == _userSearchForPat)))
                .Returns((string ldapQuery) =>
                {
                    return principals.Where(p => p.SchemaClassName.Equals("user") &&
                                                 ((Dictionary<string, string>)p.Properties).Any(pv => pv.Value.ToString().StartsWith("pat")));
                });

            return mockAdProxy;
        }
    }
}
