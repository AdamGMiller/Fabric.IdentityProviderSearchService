using System;
using System.Collections.Generic;
using System.Linq;
using Fabric.IdentityProviderSearchService.Constants;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Moq;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{

    public static class AzureDirectoryMockExtensions
    {
        private static string getITGroupWildCard =
        "startswith(DisplayName, 'ITGroup')";

        private static string getITExact =
        "DisplayName eq 'IT'";

        private static readonly string directorySearchForIT = "IT";

        private static readonly string directorySearchForITGroup = "ITGroup";

        private static string getUserWildCard =
        "startswith(DisplayName, 'johnny') or startswith(GivenName, 'johnny') or startswith(UserPrincipalName, 'johnny') or startswith(Surname, 'johnny')";

        private static string getUserExact =
        "DisplayName eq 'johnny depp'";

        private static readonly string directorySearchForUserJohnny = "johnny";

        private static readonly string directorySearchForUserJohnnyD = "johnny depp";


        private static readonly Func<FabricGraphApiGroup, string, bool> AzureGroupSearchStartsWithPredicate =
            (fg, searchText) =>
                fg.Group.DisplayName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);

        private static readonly Func<FabricGraphApiGroup, string, bool> AzureGroupSearchEqualsPredicate =
            (fg, searchText) =>
                fg.Group.DisplayName.Equals(searchText, StringComparison.OrdinalIgnoreCase);

        private static readonly Func<FabricGraphApiUser, string, string, bool> AzureUserSearchStartsWithPredicate =
            (fg, searchText, tenant) =>
                fg.TenantId.Equals(tenant, StringComparison.OrdinalIgnoreCase) && fg.User.DisplayName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase) ||
                fg.User.GivenName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase) ||
                fg.User.UserPrincipalName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase) ||
                fg.User.Surname.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);

        private static readonly Func<FabricGraphApiUser, string, bool> AzureUserSearchEqualsPredicate =
            (fg, searchText) =>
                fg.User.DisplayName.Equals(searchText, StringComparison.OrdinalIgnoreCase);


        public static Mock<IMicrosoftGraphApi> SetupAzureDirectoryGraphGroups(this Mock<IMicrosoftGraphApi> mockAdGraphGroups, IEnumerable<FabricGraphApiGroup> principals)
        {
            mockAdGraphGroups.Setup(p => p.GetGroupCollectionsAsync(getITGroupWildCard, null))
            .Returns((string filterQuery, string tenantId) =>
            {
                return Task.FromResult(principals.Where(g => AzureGroupSearchStartsWithPredicate(g, directorySearchForITGroup)));
            });

            mockAdGraphGroups.Setup(p => p.GetGroupCollectionsAsync(getITGroupWildCard, "1"))
            .Returns((string filterQuery, string tenantId) =>
            {
                return Task.FromResult(principals.Where(g => AzureGroupSearchStartsWithPredicate(g, directorySearchForITGroup)));
            });
            
            mockAdGraphGroups.Setup(p => p.GetGroupCollectionsAsync(getITExact, null))
            .Returns(() =>
            {
                var groupEntry =
                    principals.FirstOrDefault(p =>
                        AzureGroupSearchEqualsPredicate(p, directorySearchForIT));

                if (groupEntry == null)
                {
                    return null;
                }

                List<FabricGraphApiGroup> group = new List<FabricGraphApiGroup>();
                group.Add(groupEntry);
                return Task.FromResult((IEnumerable<FabricGraphApiGroup>)group);
            });

            mockAdGraphGroups.Setup(p => p.GetGroupCollectionsAsync(getITExact, "2"))
            .Returns(() =>
            {
                var groupEntry =
                    principals.FirstOrDefault(p =>
                        AzureGroupSearchEqualsPredicate(p, directorySearchForIT));

                if (groupEntry == null)
                {
                    return null;
                }

                List<FabricGraphApiGroup> group = new List<FabricGraphApiGroup>();
                group.Add(groupEntry);
                return Task.FromResult((IEnumerable<FabricGraphApiGroup>)group);
            });

            return mockAdGraphGroups;
        }

        public static Mock<IMicrosoftGraphApi> SetupAzureDirectoryGraphUsers(this Mock<IMicrosoftGraphApi> mockAdGraphUsers, IEnumerable<FabricGraphApiUser> principals)
        {
            mockAdGraphUsers.Setup(p => p.GetUserCollectionsAsync(getUserWildCard, null))
            .Returns((string filterQuery, string tenantId) =>
            {
                return Task.FromResult(principals.Where(g => g.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase) && 
                                                             AzureUserSearchStartsWithPredicate(g, directorySearchForUserJohnny, tenantId)));
            });

            mockAdGraphUsers.Setup(p => p.GetUserCollectionsAsync(getUserWildCard, "1"))
            .Returns((string filterQuery, string tenantId) =>
            {
                return Task.FromResult(principals.Where(g => g.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase) && 
                                                             AzureUserSearchStartsWithPredicate(g, directorySearchForUserJohnny, tenantId)));
            });

            mockAdGraphUsers.Setup(p => p.GetUserCollectionsAsync(getUserWildCard, "2"))
            .Returns((string filterQuery, string tenantId) =>
            {
                return Task.FromResult(principals.Where(g => g.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase) &&
                                                             AzureUserSearchStartsWithPredicate(g, directorySearchForUserJohnny, tenantId)));
            });

            mockAdGraphUsers.Setup(p => p.GetUserCollectionsAsync(getUserExact, null))
            .Returns(() =>
            {
                var userEntry =
                    principals.FirstOrDefault(p =>
                        AzureUserSearchEqualsPredicate(p, directorySearchForUserJohnnyD));

                if (userEntry == null)
                {
                    return null;
                }

                List<FabricGraphApiUser> user = new List<FabricGraphApiUser>();
                user.Add(userEntry);
                return Task.FromResult((IEnumerable<FabricGraphApiUser>)user);
            });

            mockAdGraphUsers.Setup(p => p.GetUserCollectionsAsync(getUserExact, "2"))
            .Returns(() =>
            {
                var userEntry =
                    principals.FirstOrDefault(p =>
                        AzureUserSearchEqualsPredicate(p, directorySearchForUserJohnnyD));

                if (userEntry == null)
                {
                    return null;
                }

                List<FabricGraphApiUser> user = new List<FabricGraphApiUser>();
                user.Add(userEntry);
                return Task.FromResult((IEnumerable<FabricGraphApiUser>)user);
            });

            return mockAdGraphUsers;
        }
    }
}
