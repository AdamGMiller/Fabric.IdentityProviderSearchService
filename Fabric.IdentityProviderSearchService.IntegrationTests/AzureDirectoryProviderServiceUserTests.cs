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
    public class AzureDirectoryProviderServiceUserTests
    {
        private Mock<IMicrosoftGraphApi> _mockGraphClient;
        private IEnumerable<FabricGraphApiUser> _allUsers;
        private IEnumerable<FabricGraphApiUser> _emptyUsers;
        private IEnumerable<FabricGraphApiUser> _oneUserResult;
        private FabricGraphApiUser _firstUser;
        private readonly AzureDirectoryProviderService _providerService;
        private readonly string _userFilterQuery = "startswith(DisplayName, '{0}') or startswith(GivenName, '{0}') or startswith(UserPrincipalName, '{0}')";

        public AzureDirectoryProviderServiceUserTests()
        {
            _mockGraphClient = new Mock<IMicrosoftGraphApi>();
            _allUsers = new ActiveDirectoryDataHelper().GetMicrosoftGraphUsers();
            _firstUser = _allUsers.First();
            _emptyUsers = new List<FabricGraphApiUser>();
            _oneUserResult = new List<FabricGraphApiUser>() { _firstUser };

            _mockGraphClient.Setup(p => p.GetUserCollectionsAsync(It.IsAny<string>()))
                            .Returns(Task.FromResult(_emptyUsers));
            var filterSetting = String.Format(_userFilterQuery, _firstUser.User.DisplayName);
            _mockGraphClient.Setup(p => p.GetUserCollectionsAsync(filterSetting))
                            .Returns(Task.FromResult(_oneUserResult));

            _mockGraphClient.Setup(p => p.GetUserAsync(_firstUser.User.Id))
                            .Returns(Task.FromResult(_firstUser));

            _providerService = new AzureDirectoryProviderService(_mockGraphClient.Object);
        }

        [Fact]
        public void FindUserBySubjectId_ValidId_Success()
        {
            var user = _providerService.FindUserBySubjectId(_firstUser.User.Id);

            Assert.NotNull(user);
            Assert.Equal(_firstUser.User.DisplayName, user.FirstName);
            Assert.Equal(PrincipalType.User, user.PrincipalType);
        }

        [Fact]
        public void FindUserBySubjectId_ValidIdUser_Success()
        {
            var user = _providerService.SearchPrincipals(_firstUser.User.DisplayName, PrincipalType.User);

            Assert.NotNull(user);
            Assert.True(1 == user.Count());
            Assert.Equal(_firstUser.User.Id, user.First().SubjectId);
            Assert.Equal(PrincipalType.User, user.First().PrincipalType);
        }

        [Fact]
        public void FindUserBySubjectId_InvalidSubjectIdFormat_NullResult()
        {
            var user = _providerService.FindUserBySubjectId($"not found");

            Assert.Null(user);
        }
        
        [Fact]
        public void FindUserBySubjectId_InvalidSubjectIdFormatUser_NullResult()
        {
            var principals = _providerService.SearchPrincipals($"not found", PrincipalType.User);

            Assert.NotNull(principals);
            Assert.True(principals.Count() == 0);
        }
    }
}
