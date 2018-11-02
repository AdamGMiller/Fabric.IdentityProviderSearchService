﻿using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Services;
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
    public class AzureDirectoryProviderServiceUserTests
    {
        private Mock<IMicrosoftGraphApi> _mockGraphClient;
        private IEnumerable<FabricGraphApiUser> _allUsers;
        private IEnumerable<FabricGraphApiUser> _emptyUsers;
        private IEnumerable<FabricGraphApiUser> _oneUserResult;
        private FabricGraphApiUser _firstUser;
        private readonly AzureDirectoryProviderService _providerService;
        private readonly string _userFilterWildQuery = "startswith(DisplayName, '{0}') or startswith(GivenName, '{0}') or startswith(UserPrincipalName, '{0}') or startswith(Surname, '{0}')";
        private readonly string _userFilterExactQuery = "DisplayName eq '{0}' or GivenName eq '{0}' or UserPrincipalName eq '{0}' or Surname eq '{0}'";
        private readonly string _identityProvider = "TestIdentityProvider";
        private readonly string _directorySearchForJason = "jason soto";

        private static readonly Func<FabricGraphApiUser, string, bool> AzureSearchEqualsPredicate =
            (u, searchText) =>
                u.User.DisplayName.Equals(searchText, StringComparison.OrdinalIgnoreCase);

        public AzureDirectoryProviderServiceUserTests()
        {
            _mockGraphClient = new Mock<IMicrosoftGraphApi>();
            _allUsers = new ActiveDirectoryDataHelper().GetMicrosoftGraphUsers();
            _firstUser = _allUsers.First();
            _emptyUsers = new List<FabricGraphApiUser>();
            _oneUserResult = new List<FabricGraphApiUser>() { _firstUser };

            _mockGraphClient.Setup(p => p.GetUserCollectionsAsync(It.IsAny<string>(), null))
                            .Returns(Task.FromResult(_emptyUsers));
            var filterWildSetting = String.Format(_userFilterWildQuery, _firstUser.User.DisplayName);
            var filterExactSetting = String.Format(_userFilterExactQuery, _firstUser.User.DisplayName);
            _mockGraphClient.Setup(p => p.GetUserCollectionsAsync(filterWildSetting, null))
                            .Returns(Task.FromResult(_oneUserResult));

            _mockGraphClient.Setup(p => p.GetUserCollectionsAsync(filterExactSetting, null))
                            .Returns(() =>
                            {
                               var userEntry =
                               _allUsers.FirstOrDefault(p =>
                                   AzureSearchEqualsPredicate(p, _directorySearchForJason));

                                if (userEntry == null)
                                {
                                    return null;
                                }

                                List<FabricGraphApiUser> user = new List<FabricGraphApiUser>();
                                user.Add(userEntry);
                                return Task.FromResult((IEnumerable<FabricGraphApiUser>)user);
                            });

            _mockGraphClient.Setup(p => p.GetUserAsync(_firstUser.User.Id, null))
                            .Returns(Task.FromResult(_firstUser));

            _providerService = new AzureDirectoryProviderService(_mockGraphClient.Object);
        }

        [Fact]
        public async Task FindUserBySubjectId_ValidId_SuccessAsync()
        {
            var user = await _providerService.FindUserBySubjectIdAsync(_firstUser.User.Id);

            Assert.NotNull(user);
            Assert.Equal(_firstUser.User.DisplayName, user.FirstName);
            Assert.Equal(PrincipalType.User, user.PrincipalType);
        }

        [Fact]
        public async Task FindUserBySubjectId_ValidUserWild_SuccessAsync()
        {
            var user = await _providerService.SearchPrincipalsAsync<IFabricPrincipal>(_firstUser.User.DisplayName, PrincipalType.User, SearchTypes.Wildcard);

            Assert.NotNull(user);
            Assert.True(1 == user.Count());
            Assert.Equal(_firstUser.User.UserPrincipalName, user.First().SubjectId);
            Assert.Equal(PrincipalType.User, user.First().PrincipalType);
        }

        [Fact]
        public async Task FindUserBySubjectId_ValidUserExact_SuccessAsync()
        {
            var user = await _providerService.SearchPrincipalsAsync<IFabricPrincipal>(_firstUser.User.DisplayName, PrincipalType.User, SearchTypes.Exact);

            Assert.NotNull(user);
            Assert.True(1 == user.Count());
            Assert.Equal(_firstUser.User.UserPrincipalName, user.First().SubjectId);
            Assert.Equal(PrincipalType.User, user.First().PrincipalType);
        }

        [Fact]
        public async Task FindUserBySubjectId_InvalidSubjectIdFormat_NullResultAsync()
        {
            var user = await _providerService.FindUserBySubjectIdAsync($"not found");

            Assert.Null(user);
        }
        
        [Fact]
        public async Task FindUserBySubjectId_InvalidSubjectIdFormatUser_NullResultAsync()
        {
            var principals = await _providerService.SearchPrincipalsAsync<IFabricPrincipal>($"not found", PrincipalType.User, SearchTypes.Exact);

            Assert.NotNull(principals);
            Assert.True(principals.Count() == 0);
        }
    }
}
