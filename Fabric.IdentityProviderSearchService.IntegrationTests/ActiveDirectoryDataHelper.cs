using System.Collections.Generic;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Models;
using Microsoft.Graph;
using Moq;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public class ActiveDirectoryDataHelper
    {
        public IEnumerable<IDirectoryEntry> GetPrincipals()
        {
            var principals = new List<IDirectoryEntry>
            {
                CreateMockDirectoryEntry("patrick", "jones", PrincipalType.User, "Patrick Jones"),
                CreateMockDirectoryEntry("patricia", "smith", PrincipalType.User, "Patricia Smith"),
                CreateMockDirectoryEntry("janet", "apple", PrincipalType.User, "Janet Apple"),
                CreateMockDirectoryEntry("", "", PrincipalType.Group, "patient group"),
                CreateMockDirectoryEntry("", "", PrincipalType.Group, "janitorial group"),
                CreateMockDirectoryEntry("", "", PrincipalType.Group, "developer group")
            };

            return principals;
        }

        private static IDirectoryEntry CreateMockDirectoryEntry(string firstName, string lastName, PrincipalType type, string name = "")
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

        public IEnumerable<FabricGraphApiUser> GetMicrosoftGraphUsers()
        {
            var principals = new List<FabricGraphApiUser>
            {
                CreateMicrosoftGraphUser("1", "jason soto"),
                CreateMicrosoftGraphUser("1", "jorden lowe"),
                CreateMicrosoftGraphUser("1", "ryan orbaker"),
                CreateMicrosoftGraphUser("1", "michael vidal"),
                CreateMicrosoftGraphUser("1", "brian smith"),
                CreateMicrosoftGraphUser("1", "ken miller")
            };

            return principals;
        }

        private static FabricGraphApiUser CreateMicrosoftGraphUser(string id, string displayName)
        {
            var user = new User()
            {
                UserPrincipalName = displayName,
                GivenName = displayName,
                DisplayName = displayName,
                Surname = displayName,
                Id = id
            };

            return new FabricGraphApiUser(user)
            {
                TenantId = "someId"
            };
        }
        
        public IEnumerable<FabricGraphApiGroup> GetMicrosoftGraphGroups()
        {
            var principals = new List<FabricGraphApiGroup>
            {
                CreateMicrosoftGraphGroup("1", "IT"),
                CreateMicrosoftGraphGroup("1", "Fabric")
            };

            return principals;
        }

        private static FabricGraphApiGroup CreateMicrosoftGraphGroup(string id, string displayName)
        {
            var group = new Group
            {
                DisplayName = displayName,
                Id = id
            };
            return new FabricGraphApiGroup(group)
            {
                TenantId = "someId"
            };
        }
    }
}
