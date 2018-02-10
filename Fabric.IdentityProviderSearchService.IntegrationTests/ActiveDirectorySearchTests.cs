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
    public class ActiveDirectorySearchTests : IClassFixture<IntegrationTestsFixture>
    {
        private readonly Browser _browser;

        public ActiveDirectorySearchTests(IntegrationTestsFixture integrationTestsFixture)
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("scope", Scopes.SearchPrincipalsScope)
            }, "testprincipal"));

            _browser = integrationTestsFixture.GetBrowser(claimsPrincipal);
        }

        [Fact]
        public async Task FindUsersAndGroups_Succeeds_Async()
        {
            var searchResult = await _browser.Get("/principals/search", with =>
            {
                with.HttpRequest();
                with.Query("searchtext", "pat");
            });

            Assert.Equal(HttpStatusCode.OK, searchResult.StatusCode);
            
            var users = searchResult.Body.DeserializeJson<IdpSearchResultApiModel>();
            Assert.Equal(3, users.ResultCount);
            Assert.Equal(2, users.Principals.Count(p => p.PrincipalType.Equals("user")));
            Assert.Equal(1, users.Principals.Count(p => p.PrincipalType.Equals("group")));
        }

        [Fact]
        public async Task FindGroups_Succeeds_Async()
        {
            var searchResult = await _browser.Get("/principals/search", with =>
            {
                with.HttpRequest();
                with.Query("searchtext", "pat");
                with.Query("type", "group");
            });

            Assert.Equal(HttpStatusCode.OK, searchResult.StatusCode);

            var users = searchResult.Body.DeserializeJson<IdpSearchResultApiModel>();
            Assert.Equal(1, users.ResultCount);            
            Assert.Equal(1, users.Principals.Count(p => p.PrincipalType.Equals("group")));
        }

        [Fact]
        public async Task FindUsers_Succeeds_Async()
        {
            var searchResult = await _browser.Get("/principals/search", with =>
            {
                with.HttpRequest();
                with.Query("searchtext", "pat");
                with.Query("type", "user");
            });

            Assert.Equal(HttpStatusCode.OK, searchResult.StatusCode);

            var users = searchResult.Body.DeserializeJson<IdpSearchResultApiModel>();
            Assert.Equal(2, users.ResultCount);
            Assert.Equal(2, users.Principals.Count(p => p.PrincipalType.Equals("user")));
        }
    }
}
