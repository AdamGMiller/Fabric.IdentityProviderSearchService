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
            principal1.SetupGet(p => p.FirstName).Returns(firstName);
            principal1.SetupGet(p => p.LastName).Returns(lastName);
            principal1.SetupGet(p => p.MiddleName).Returns("middlename");
            principal1.SetupGet(p => p.Name).Returns(name);
            principal1.SetupGet(p => p.SamAccountName)
                .Returns(type == PrincipalType.User ? $"{firstName}.{lastName}" : name);

            return principal1.Object;
        }        
    }
}
