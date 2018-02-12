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
                CreateMockDirectoryEntry("patrick", "jones", PrincipalType.User),
                CreateMockDirectoryEntry("patricia", "smith", PrincipalType.User),
                CreateMockDirectoryEntry("janet", "apple", PrincipalType.User),
                CreateMockDirectoryEntry("", "", PrincipalType.Group, "patient group"),
                CreateMockDirectoryEntry("", "", PrincipalType.Group, "janitorial group"),
                CreateMockDirectoryEntry("", "", PrincipalType.Group, "developer group")
            };

            return principals;
        }

        private IDirectoryEntry CreateMockDirectoryEntry(string firstName, string lastName, PrincipalType type, string name = "")
        {
            var principal1 = new Mock<IDirectoryEntry>();
            principal1.SetupGet(p => p.SchemaClassName).Returns(type.ToString().ToLower());
            principal1.SetupGet(p => p.Properties).Returns(() =>
            {
                var props = new Dictionary<string, string>
                {
                    {"givenname", firstName},
                    {"sn", lastName},
                    {"middlename", "middlename"},
                    {"samaccountname", type == PrincipalType.User ? $"{firstName}.{lastName}" : name},
                    {"name", name }
                };

                return props;
            });

            return principal1.Object;
        }        
    }
}
