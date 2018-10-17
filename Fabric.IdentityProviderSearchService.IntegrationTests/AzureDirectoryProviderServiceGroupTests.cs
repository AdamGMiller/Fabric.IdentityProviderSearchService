using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services;
using Microsoft.Graph;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public class AzureDirectoryProviderServiceGroupTests
    {
        private Mock<IMicrosoftGraphApi> _mockGraphClient;
        private IEnumerable<Group> _allGroups;
        private IEnumerable<Group> _emptyGroups;
        private IEnumerable<Group> _oneGroupResult;
        private Group _firstGroup;
        private readonly AzureDirectoryProviderService _providerService;
        private readonly string _groupFilterQuery = "startswith(DisplayName, '{0}')";

        public AzureDirectoryProviderServiceGroupTests()
        {
            _mockGraphClient = new Mock<IMicrosoftGraphApi>();
            _allGroups = new ActiveDirectoryDataHelper().GetMicrosoftGraphGroups();
            _firstGroup = _allGroups.First();
            _emptyGroups = new List<Group>();
            _oneGroupResult = new List<Group>() { _firstGroup };

            _mockGraphClient.Setup(p => p.GetGroupCollectionsAsync(It.IsAny<string>()))
                            .Returns(Task.FromResult(_emptyGroups));
            var filterSetting = String.Format(_groupFilterQuery, _firstGroup.DisplayName);
            _mockGraphClient.Setup(p => p.GetGroupCollectionsAsync(filterSetting))
                            .Returns(Task.FromResult(_oneGroupResult));
            
            _providerService = new AzureDirectoryProviderService(_mockGraphClient.Object);
        }
        
        [Fact]
        public void FindGroupBySubjectId_ValidIdGroup_Success()
        {
            var Group = _providerService.SearchPrincipals(_firstGroup.DisplayName, PrincipalType.Group);

            Assert.NotNull(Group);
            Assert.True(1 == Group.Count());
            Assert.Equal(_firstGroup.Id, Group.First().SubjectId);
            Assert.Equal(PrincipalType.Group, Group.First().PrincipalType);
        }
                
        [Fact]
        public void FindGroupBySubjectId_InvalidSubjectIdFormatGroup_NullResult()
        {
            var principals = _providerService.SearchPrincipals($"not found", PrincipalType.Group);

            Assert.NotNull(principals);
            Assert.True(principals.Count() == 0);
        }
    }
}
