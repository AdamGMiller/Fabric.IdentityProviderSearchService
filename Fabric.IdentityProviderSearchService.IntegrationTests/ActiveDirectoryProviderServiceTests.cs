using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services;
using Moq;
using Xunit;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public class ActiveDirectoryProviderServiceTests
    {
        private readonly AppConfiguration _appConfig;
        private readonly ActiveDirectoryProviderService _providerService;

        public ActiveDirectoryProviderServiceTests()
        {
            var activeDirectoryProxyMock = new Mock<IActiveDirectoryProxy>()
                .SetupActiveDirectoryProxy(new ActiveDirectoryDataHelper().GetPrincipals());

            _appConfig = new AppConfiguration
            {
                DomainName = "testing"
            };

            _providerService = new ActiveDirectoryProviderService(activeDirectoryProxyMock.Object, _appConfig);
        }

        [Fact]
        public void FindUserBySubjectId_ValidId_Success()
        {
            var user = _providerService.FindUserBySubjectId($"{_appConfig.DomainName}\\patrick.jones");

            Assert.NotNull(user);
            Assert.Equal("patrick", user.FirstName);
            Assert.Equal("jones", user.LastName);
            Assert.Equal(PrincipalType.User, user.PrincipalType);
        }

        [Fact]
        public void FindUserBySubjectId_InvalidSubjectIdFormat_NullResult()
        {
            var user = _providerService.FindUserBySubjectId($"{_appConfig.DomainName}.patrick.jones");

            Assert.Null(user); 
        }

        [Fact]
        public void FindUserBySubjectId_UserNotFound_NullResult()
        {
            var user = _providerService.FindUserBySubjectId($"{_appConfig.DomainName}\\patrick.jon");

            Assert.Null(user);
        }
    }
}
