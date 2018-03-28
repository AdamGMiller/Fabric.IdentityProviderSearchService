﻿namespace Fabric.IdentityProviderSearchService.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Fabric.IdentityProviderSearchService.Models;

    using Newtonsoft.Json;

    /// <summary>
    /// Client for interacting with the DiscoveryService.
    /// </summary>
    public class DiscoveryServiceClient : IDisposable
    {
        /// <summary>
        /// The HttpClient for making HTTP calls.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryServiceClient"/> class.
        /// </summary>
        /// <param name="discoveryServiceUrl">The URL (including version) of the DiscoveryService.</param>
        public DiscoveryServiceClient(string discoveryServiceUrl) : this(discoveryServiceUrl, new HttpClientHandler { UseDefaultCredentials = true})
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryServiceClient"/> class.
        /// </summary>
        /// <param name="discoveryServiceUrl">The URL (including version) of the DiscoveryService.</param>
        /// <param name="handler">The optional message handler for processing requests.</param>
        public DiscoveryServiceClient(string discoveryServiceUrl, HttpMessageHandler handler)
        {
            this.httpClient = new HttpClient(handler) { BaseAddress = new Uri(this.FormatUrl(discoveryServiceUrl)) };
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Registers a service with DiscoveryService.
        /// </summary>
        /// <param name="discoveryServiceApi">The <see cref="DiscoveryServiceApiModel"/> to register.</param>
        /// <returns>A boolean value indicating success.</returns>
        public Task<bool> RegisterServiceAsync(DiscoveryServiceApiModel discoveryServiceApi)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the registration for a service from DiscoveryService.
        /// </summary>
        /// <param name="serviceName">The name of the service to retrieve.</param>
        /// <param name="serviceVersion">The version of the service to retrieve.</param>
        /// <returns>A <see cref="DiscoveryServiceApiModel"/></returns>
        public async Task<DiscoveryServiceApiModel> GetServiceAsync(string serviceName, int serviceVersion)
        {
            var url = $"Services(ServiceName='{serviceName}', Version={serviceVersion})";
            var response = await this.httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var apiModel = JsonConvert.DeserializeObject<DiscoveryServiceApiModel>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            return apiModel;
        }
        
        /// <summary>
        /// Cleans up resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases managed resources. 
        /// </summary>
        /// <param name="disposing">Flag to indicate whether to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.httpClient?.Dispose();
            }
        }

        /// <summary>
        /// Adds a trailing slash to the url if it is not present.
        /// </summary>
        /// <param name="url">The url to format.</param>
        /// <returns>The formatted url.</returns>
        private string FormatUrl(string url)
        {
            return !url.EndsWith("/") ? $"{url}/" : url;
        }
    }
}