using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.ApiModels;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services;
using Moq;
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
        public async Task FindUsers_Succeeds_WhenUsersExistAsync()
        {
            var searchResult = await _browser.Get($"/principals/search", with =>
            {
                with.HttpRequest();
                with.Query("searchtext", "user");
            });

            Assert.Equal(HttpStatusCode.OK, searchResult.StatusCode);
            
            var users = searchResult.Body.DeserializeJson<IdpSearchResultApiModel>();
            Assert.Equal(3, users.ResultCount);

            
        }
    }

    

    
}
