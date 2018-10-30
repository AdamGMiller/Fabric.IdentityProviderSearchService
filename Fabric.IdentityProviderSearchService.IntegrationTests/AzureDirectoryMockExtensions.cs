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
        private static IEnumerable<FabricGraphApiGroup> _allGroupResult;

        private static IEnumerable<FabricGraphApiGroup> _allGroups;

        private static IEnumerable<FabricGraphApiGroup> _listGroups;

        private static string getITGroupWildCard =
        "startswith(DisplayName, 'ITGroup')";

        private static string getITExact =
        "DisplayName eq 'IT'";

        private static readonly string directorySearchForIT = "IT";

        private static readonly string directorySearchForITGroup = "ITGroup";


        private static readonly Func<FabricGraphApiGroup, string, bool> AzureSearchStartsWithPredicate =
            (fg, searchText) =>
                fg.Group.DisplayName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);

        private static readonly Func<FabricGraphApiGroup, string, bool> AzureSearchEqualsPredicate =
            (fg, searchText) =>
                fg.Group.DisplayName.Equals(searchText, StringComparison.OrdinalIgnoreCase);


        public static Mock<IMicrosoftGraphApi> SetupAzureDirectoryGraph(this Mock<IMicrosoftGraphApi> mockAdGraph, IEnumerable<FabricGraphApiGroup> principals)
        {
            mockAdGraph.Setup(p => p.GetGroupCollectionsAsync(getITGroupWildCard, null))
            .Returns((string filterQuery, string tenantId) =>
            {
                return Task.FromResult(principals.Where(g => g.Group.DisplayName.Contains(directorySearchForITGroup) &&
                                             AzureSearchStartsWithPredicate(g, directorySearchForITGroup)));
            });

            mockAdGraph.Setup(p => p.GetGroupCollectionsAsync(getITGroupWildCard, "1"))
            .Returns((string filterQuery, string tenantId) =>
            {
                return Task.FromResult(principals.Where(g => g.Group.DisplayName.Contains(directorySearchForITGroup) &&
                                             AzureSearchStartsWithPredicate(g, directorySearchForITGroup)));
            });
            
            mockAdGraph.Setup(p => p.GetGroupCollectionsAsync(getITExact, null))
            .Returns(() =>
            {
                var groupEntry =
                    principals.FirstOrDefault(p =>
                        AzureSearchEqualsPredicate(p, directorySearchForIT));

                if (groupEntry == null)
                {
                    return null;
                }

                List<FabricGraphApiGroup> group = new List<FabricGraphApiGroup>();
                group.Add(groupEntry);
                return Task.FromResult((IEnumerable<FabricGraphApiGroup>)group);
            });

            mockAdGraph.Setup(p => p.GetGroupCollectionsAsync(getITExact, "2"))
            .Returns(() =>
            {
                var groupEntry =
                    principals.FirstOrDefault(p =>
                        AzureSearchEqualsPredicate(p, directorySearchForIT));

                if (groupEntry == null)
                {
                    return null;
                }

                List<FabricGraphApiGroup> group = new List<FabricGraphApiGroup>();
                group.Add(groupEntry);
                return Task.FromResult((IEnumerable<FabricGraphApiGroup>)group);
            });

            return mockAdGraph;
        }
    }
}
