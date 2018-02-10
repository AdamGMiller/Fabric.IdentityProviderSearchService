using System.Collections.Generic;
using Fabric.IdentityProviderSearchService.Models;
using Moq;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public class ActiveDirectoryDataHelper
    {
        public IEnumerable<IDirectoryEntry> GetPrincipals()
        {
            var principals = new List<IDirectoryEntry>
            {
                CreateMockDirectoryEntry("user1", PrincipalType.User),
                CreateMockDirectoryEntry("user2", PrincipalType.User),
                CreateMockDirectoryEntry("user3", PrincipalType.User),
                CreateMockDirectoryEntry("group1", PrincipalType.Group, "group one"),
                CreateMockDirectoryEntry("group2", PrincipalType.Group, "group two"),
                CreateMockDirectoryEntry("group3", PrincipalType.Group, "group three")
            };

            return principals;
        }

        private IDirectoryEntry CreateMockDirectoryEntry(string id, PrincipalType type, string name = "")
        {
            var principal1 = new Mock<IDirectoryEntry>();
            principal1.SetupGet(p => p.SchemaClassName).Returns(type.ToString().ToLower());
            principal1.SetupGet(p => p.Properties).Returns(() =>
            {
                var props = new Dictionary<string, string>
                {
                    {"givenname", id},
                    {"sn", $"{id}sn"},
                    {"middlename", $"{id}middlename"},
                    {"samaccountname", $"{id}.{id}sn"},
                    {"name", name }
                };

                return props;
            });

            return principal1.Object;
        }
    }
}
