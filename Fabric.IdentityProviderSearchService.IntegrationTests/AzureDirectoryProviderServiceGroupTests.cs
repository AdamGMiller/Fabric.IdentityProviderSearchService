﻿using Fabric.IdentityProviderSearchService.Constants;
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
        private IEnumerable<FabricGraphApiGroup> _allGroupResult;
        private FabricGraphApiGroup _firstGroup;
        private IEnumerable<FabricGraphApiGroup> _listGroups;
        private AzureDirectoryProviderService _providerService;
        private readonly string _groupWildFilterQuery = "startswith(DisplayName, '{0}')";
        private readonly string _groupExactFilterQuery = "DisplayName eq '{0}'";
        private readonly string _identityProvider = "TestIdentityProvider";

        public AzureDirectoryProviderServiceGroupTests()
        {
            _mockGraphClient = new Mock<IMicrosoftGraphApi>();
            _allGroups = new ActiveDirectoryDataHelper().GetMicrosoftGraphGroups();
            _firstGroup = _allGroups.First();
            _listGroups = _allGroups.ToList();
            _emptyGroups = new List<FabricGraphApiGroup>();
            _oneGroupResult = new List<FabricGraphApiGroup>() { _firstGroup };

            _mockGraphClient.Setup(p => p.GetGroupCollectionsAsync(It.IsAny<string>(), null))
                            .Returns(Task.FromResult(_emptyGroups));
        }
        
        [Fact]
        public async Task FindGroupByGroupName_ValidGroup_SuccessAsync()
        {
            var searchText = _listGroups.First(g => g.Group.DisplayName == "Fabric").Group.DisplayName;
            this.SetupGraphClient(searchText, "Exact");

            var Group = await  _providerService.SearchPrincipalsAsync<IFabricGroup>(searchText, PrincipalType.Group, SearchTypes.Exact, _identityProvider);

            Assert.NotNull(Group);
            Assert.True(1 == Group.Count());
            Assert.Equal(_firstGroup.Group.Id, Group.First().GroupId);
            Assert.Equal(PrincipalType.Group, Group.First().PrincipalType);
        }

        [Fact]
        public async Task FindGroupByGroupName_ValidGroups_SuccessAsync()
        {
            var searchText = _listGroups.First(g => g.Group.DisplayName == "ITGroup").Group.DisplayName;
            this.SetupGraphClient(searchText, "Exact");
            var principals = await _providerService.SearchPrincipalsAsync<IFabricGroup>(searchText, PrincipalType.Group, SearchTypes.Exact, _identityProvider);

            Assert.NotNull(principals);
            Assert.True(principals.Count() == 2);
        }

        [Fact]
        public async Task FindGroupByGroupName_InvalidGroupName_NullResultAsync()
        {
            this.SetupGraphClient("not found", "Exact");
            var principals = await _providerService.SearchPrincipalsAsync<IFabricGroup>($"not found", PrincipalType.Group, SearchTypes.Exact, _identityProvider);

            Assert.NotNull(principals);
            Assert.True(principals.Count() == 0);
        }

        [Fact]
        public async Task FindGroupsThatContainGroupName_InvalidGroupName_NullResultAsync()
        {
            this.SetupGraphClient("not found", "Wild");
            var principals = await _providerService.SearchPrincipalsAsync<IFabricGroup>($"not found", PrincipalType.Group, SearchTypes.Wildcard, _identityProvider);

            Assert.NotNull(principals);
            Assert.True(principals.Count() == 0);
        }

        [Fact]
        public async Task FindGroupThatContainsGroupName_ValidGroup_SuccessAsync()
        {
            var searchText = _listGroups.First(g => g.Group.DisplayName == "ITGroup").Group.DisplayName;
            this.SetupGraphClient(searchText, "Wild");
            var principals = await _providerService.SearchPrincipalsAsync<IFabricGroup>(searchText, PrincipalType.Group, SearchTypes.Wildcard, _identityProvider);

            Assert.NotNull(principals);
            Assert.True(principals.Count() == 4);
        }

        public void SetupGraphClient (string searchText = null, string searchFilter = null)
        {
                if (searchFilter == "Wild")
                {
                    _allGroupResult = _listGroups.Where(g => g.Group.DisplayName.Contains(searchText));
                    if (_allGroupResult.Count() > 0)
                    {
                        var filterSettingWild = String.Format(_groupWildFilterQuery, _allGroupResult.First().Group.DisplayName);
                        _mockGraphClient.Setup(p => p.GetGroupCollectionsAsync(filterSettingWild, null))
                                        .Returns(Task.FromResult(_allGroupResult));
                    }
                }
                else if (searchFilter == "Exact")
                {
                    _allGroupResult = _listGroups.Where(g => g.Group.DisplayName == searchText);
                    if (_allGroupResult.Count() > 0)
                    {
                        var filterSettingExact = String.Format(_groupExactFilterQuery, _allGroupResult.First().Group.DisplayName);
                        _mockGraphClient.Setup(p => p.GetGroupCollectionsAsync(filterSettingExact, null))
                                        .Returns(Task.FromResult(_allGroupResult));
                    }
                }
            _providerService = new AzureDirectoryProviderService(_mockGraphClient.Object);
        }
    }
}
