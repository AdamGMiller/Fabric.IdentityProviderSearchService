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
                CreateMicrosoftGraphUser("2", "jorden lowe"),
                CreateMicrosoftGraphUser("3", "ryan orbaker"),
                CreateMicrosoftGraphUser("4", "michael vidal"),
                CreateMicrosoftGraphUser("5", "brian smith"),
                CreateMicrosoftGraphUser("6", "ken miller"),
                CreateMicrosoftGraphUser("7", "johnny depp", "1"),
                CreateMicrosoftGraphUser("8", "johnny cash", "1"),
                CreateMicrosoftGraphUser("9", "johnny depp", "2")
            };

            return principals;
        }

        private static FabricGraphApiUser CreateMicrosoftGraphUser(string id, string displayName, string tenantId = "null")
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
                TenantId = tenantId
            };
        }
        
        public IEnumerable<FabricGraphApiGroup> GetMicrosoftGraphGroups()
        {
            var principals = new List<FabricGraphApiGroup>
            {
                CreateMicrosoftGraphGroup("1", "IT"),
                CreateMicrosoftGraphGroup("2", "Fabric"),
                CreateMicrosoftGraphGroup("3", "ITGroup", "1"),
                CreateMicrosoftGraphGroup("4", "ITGroup", "2"),
                CreateMicrosoftGraphGroup("5", "ITGrouper", "1"),
                CreateMicrosoftGraphGroup("6", "ITGrouper", "2")
            };

            return principals;
        }

        private static FabricGraphApiGroup CreateMicrosoftGraphGroup(string id, string displayName, string tenantId = "someId")
        {
            var group = new Group
            {
                DisplayName = displayName,
                Id = id
            };
            return new FabricGraphApiGroup(group)
            {
                TenantId = tenantId
            };
        }
    }
}
