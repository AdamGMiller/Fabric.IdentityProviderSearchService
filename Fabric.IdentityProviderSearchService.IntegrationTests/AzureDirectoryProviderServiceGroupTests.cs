using Fabric.IdentityProviderSearchService.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Xunit;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public class AzureDirectoryProviderServiceGroupTests
    {
        private Mock<IMicrosoftGraphApi> _mockGraphClient;
        private IEnumerable<FabricGraphApiGroup> _allGroups;
        private IEnumerable<FabricGraphApiGroup> _emptyGroups;
        private IEnumerable<FabricGraphApiGroup> _oneGroupResult;
        private FabricGraphApiGroup _firstGroup;
        private readonly AzureDirectoryProviderService _providerService;
        private readonly string _groupFilterQuery = "startswith(DisplayName, '{0}')";

        public AzureDirectoryProviderServiceGroupTests()
        {
            _mockGraphClient = new Mock<IMicrosoftGraphApi>();
            _allGroups = new ActiveDirectoryDataHelper().GetMicrosoftGraphGroups();
            _firstGroup = _allGroups.First();
            _emptyGroups = new List<FabricGraphApiGroup>();
            _oneGroupResult = new List<FabricGraphApiGroup>() { _firstGroup };

            _mockGraphClient.Setup(p => p.GetGroupCollectionsAsync(It.IsAny<string>()))
                            .Returns(Task.FromResult(_emptyGroups));
            var filterSetting = String.Format(_groupFilterQuery, _firstGroup.Group.DisplayName);
            _mockGraphClient.Setup(p => p.GetGroupCollectionsAsync(filterSetting))
                            .Returns(Task.FromResult(_oneGroupResult));
            
            _providerService = new AzureDirectoryProviderService(_mockGraphClient.Object);
        }
        
        [Fact]
        public async Task FindGroupBySubjectId_ValidIdGroup_SuccessAsync()
        {
            var groups =
                (await _providerService.SearchPrincipalsAsync(_firstGroup.Group.DisplayName, PrincipalType.Group, SearchTypes.Wildcard))
                .ToList();

            Assert.NotNull(groups);
            Assert.True(1 == groups.Count);
            Assert.Equal(_firstGroup.Group.Id, groups.First().SubjectId);
            Assert.Equal(PrincipalType.Group, groups.First().PrincipalType);
        }
                
        [Fact]
        public async Task FindGroupBySubjectId_InvalidSubjectIdFormatGroup_NullResultAsync()
        {
            var principals = await _providerService.SearchPrincipalsAsync($"not found", PrincipalType.Group, SearchTypes.Wildcard);

            Assert.NotNull(principals);
            Assert.Empty(principals);
        }
    }
}
