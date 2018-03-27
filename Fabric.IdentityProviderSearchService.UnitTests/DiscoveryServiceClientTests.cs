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
        private Mock<MockHttpHandler> _mockHttpHandler;
        public DiscoveryServiceClientTests()
        {
            _mockHttpHandler = new Mock<MockHttpHandler>{CallBase = true};
        }
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

            var discoveryBaseUrl = "http://localhost/DiscoveryService/v1/";
            var discoverySearchUrl =
                $"{discoveryBaseUrl}Services(ServiceName='{expectedIdentityServiceModel.ServiceName}', Version={expectedIdentityServiceModel.Version})";


            _mockHttpHandler.Setup(httpHandler => httpHandler.Send(It.IsAny<HttpRequestMessage>()))
                .Returns((HttpRequestMessage requestMessage) =>
                    {
                        if (requestMessage.RequestUri.ToString()
                            .Equals(discoverySearchUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            return new HttpResponseMessage
                                       {
                                           StatusCode = HttpStatusCode.OK,
                                           Content = new StringContent(
                                               JsonConvert.SerializeObject(
                                                   expectedIdentityServiceModel))
                                       };
                        }
                        return new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.BadRequest
                            };
                    });
            
            var discoveryServiceClient = new DiscoveryServiceClient("http://localhost/DiscoveryService/v1/", _mockHttpHandler.Object);
            var identityServiceModel = await discoveryServiceClient.GetServiceAsync(
                                           expectedIdentityServiceModel.ServiceName,
                                           expectedIdentityServiceModel.Version);

            Assert.Equal(expectedIdentityServiceModel.ServiceUrl, identityServiceModel.ServiceUrl);
        }

        [Fact]
        public async Task GetService_ShouldThrow_ForNonSuccessCode()
        {
            _mockHttpHandler.Setup(httpHandler => httpHandler.Send(It.IsAny<HttpRequestMessage>()))
                .Returns((HttpRequestMessage requestMessage) =>
                    new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.BadRequest
                        });
            var discoveryServiceClient = new DiscoveryServiceClient("http://localhost/DiscoveryService/v1/", _mockHttpHandler.Object);
            await Assert.ThrowsAsync<HttpRequestException>(() => discoveryServiceClient.GetServiceAsync("Identity", 1));
        }
    }
}
