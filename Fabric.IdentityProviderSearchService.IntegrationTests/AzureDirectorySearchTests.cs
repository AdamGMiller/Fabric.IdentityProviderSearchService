using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.ApiModels;
using Fabric.IdentityProviderSearchService.Constants;
using Nancy;
using Nancy.Testing;
using Xunit;

namespace Fabric.IdentityProviderSearchService.IntegrationTests
{
    public class AzureDirectorySearchTests : IClassFixture<IntegrationTestsFixture>
    {
        private readonly IntegrationTestsFixture _integrationTestsFixture;
        private readonly Browser _browser;

        public AzureDirectorySearchTests(IntegrationTestsFixture integrationTestsFixture)
        {
            _integrationTestsFixture = integrationTestsFixture;
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("scope", Scopes.SearchPrincipalsScope)
            }, "testprincipal"));

            _browser = integrationTestsFixture.GetBrowser(claimsPrincipal);
        }

        [Fact]
        public async Task SearchPrincipals_FindGroups_Succeeds_Async()
        {
            var searchResult = await _browser.Get("/principals/Azure/search", with =>
            {
                with.HttpRequest();
                with.Query("searchtext", "ITGroup");
                with.Query("type", "group");
            });

            Assert.Equal(HttpStatusCode.OK, searchResult.StatusCode);

            var groups = searchResult.Body.DeserializeJson<IdpSearchResultApiModel<FabricGroupApiModel>>();
            Assert.Equal(4, groups.ResultCount);
            Assert.Equal(4, groups.Principals.Select(p => p.IdentityProvider.Equals("Azure")).Count());
            Assert.Equal(4, groups.Principals.Select(p => p.PrincipalType.Equals("group")).Count());
        }

        [Fact]
        public async Task SearchPrincipals_FindGroupsWithTenant_Succeeds_Async()
        {
            var searchResult = await _browser.Get("/principals/Azure/search", with =>
            {
                with.HttpRequest();
                with.Query("searchtext", "ITGroup");
                with.Query("type", "group");
                with.Query("tenantid", "1");
            });

            Assert.Equal(HttpStatusCode.OK, searchResult.StatusCode);

            var groups = searchResult.Body.DeserializeJson<IdpSearchResultApiModel<FabricGroupApiModel>>();
            Assert.Equal(4, groups.ResultCount);
            Assert.Equal(4, groups.Principals.Select(p => p.IdentityProvider.Equals("Azure")).Count());
            Assert.Equal(4, groups.Principals.Select(p => p.PrincipalType.Equals("group")).Count());
            Assert.Equal(4, groups.Principals.Select(p => p.TenantId.Equals("tenantid")).Count());
        }

        [Fact]
        public async Task SearchPrincipalsByGroup_FindGroup_Succeeds_Async()
        {
            var searchResult = await _browser.Get("/principals/Azure/groups/IT", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.OK, searchResult.StatusCode);

            var groups = searchResult.Body.DeserializeJson<IdpSearchResultApiModel<FabricGroupApiModel>>();
            Assert.Equal(1, groups.ResultCount);
            Assert.Single(groups.Principals.Select(p => p.IdentityProvider.Equals("Azure")));
            Assert.Single(groups.Principals.Select(p => p.GroupName.Equals("IT")));
            Assert.Single(groups.Principals.Select(p => p.PrincipalType.Equals("group")));
        }

        [Fact]
        public async Task SearchPrincipalsByGroup_FindGroupByTenant_Succeeds_Async()
        {
            var searchResult = await _browser.Get("/principals/Azure/groups/IT", with =>
            {
                with.HttpRequest();
                with.Query("tenantid", "2");
            });

            Assert.Equal(HttpStatusCode.OK, searchResult.StatusCode);

            var groups = searchResult.Body.DeserializeJson<IdpSearchResultApiModel<FabricGroupApiModel>>();
            Assert.Equal(1, groups.ResultCount);
            Assert.Single(groups.Principals.Select(p => p.IdentityProvider.Equals("Azure")));
            Assert.Single(groups.Principals.Select(p => p.GroupName.Equals("IT")));
            Assert.Single(groups.Principals.Select(p => p.TenantId.Equals("2")));
            Assert.Single(groups.Principals.Select(p => p.PrincipalType.Equals("group")));
        }
    }
}
