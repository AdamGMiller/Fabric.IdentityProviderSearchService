using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            _mockGraphClient.Setup(p => p.GetGroupCollectionsAsync(It.IsAny<string>(), null))
                            .Returns(Task.FromResult(_emptyGroups));
            var filterSetting = String.Format(_groupFilterQuery, _firstGroup.Group.DisplayName);
            _mockGraphClient.Setup(p => p.GetGroupCollectionsAsync(filterSetting, null))
                            .Returns(Task.FromResult(_oneGroupResult));
            
            _providerService = new AzureDirectoryProviderService(_mockGraphClient.Object);
        }
        
        [Fact]
        public async Task FindGroupBySubjectId_ValidIdGroup_SuccessAsync()
        {
            var Group = await  _providerService.SearchPrincipalsAsync<IFabricGroup>(_firstGroup.Group.DisplayName, PrincipalType.Group);

            Assert.NotNull(Group);
            Assert.True(1 == Group.Count());
            Assert.Equal(_firstGroup.Group.Id, Group.First().GroupId);
            Assert.Equal(PrincipalType.Group, Group.First().PrincipalType);
        }
                
        [Fact]
        public async Task FindGroupBySubjectId_InvalidSubjectIdFormatGroup_NullResultAsync()
        {
            var principals = await _providerService.SearchPrincipalsAsync<IFabricGroup>($"not found", PrincipalType.Group);

            Assert.NotNull(principals);
            Assert.True(principals.Count() == 0);
        }
    }
}
