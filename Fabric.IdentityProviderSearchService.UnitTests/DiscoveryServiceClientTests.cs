using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fabric.IdentityProviderSearchService.UnitTests
{
    using System.Net;
    using System.Net.Http;

    using Fabric.IdentityProviderSearchService.Models;
    using Fabric.IdentityProviderSearchService.Services;
    using Fabric.IdentityProviderSearchService.UnitTests.Mocks;

    using Moq;

    using Newtonsoft.Json;

    using Xunit;

    public class DiscoveryServiceClientTests
    {
        [Fact]
        public async Task GetService_ShouldReturn_ValidServiceAsync()
        {
            var expectedIdentityServiceModel =
                new DiscoveryServiceApiModel
                    {
                        ServiceUrl = "http://localhost/identity",
                        ServiceName = "IdentityService",
                        Version = 1,
                        DiscoveryType = "Service"
                    };

            var mockHttpHandler = new Mock<MockHttpHandler>();
            mockHttpHandler.Setup(httpHandler => httpHandler.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(
                    new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content =
                                new StringContent(JsonConvert.SerializeObject(expectedIdentityServiceModel))
                        });

            var discoveryServiceClient = new DiscoveryServiceClient("http://localhost/DiscoveryService/v1", mockHttpHandler.Object);
            var identityServiceModel = await discoveryServiceClient.GetServiceAsync(
                                           expectedIdentityServiceModel.ServiceName,
                                           expectedIdentityServiceModel.Version);

            Assert.Equal(expectedIdentityServiceModel.ServiceUrl, identityServiceModel.ServiceUrl);
        }
    }
}
