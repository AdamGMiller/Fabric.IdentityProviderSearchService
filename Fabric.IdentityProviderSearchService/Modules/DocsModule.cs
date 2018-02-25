using Nancy;
using Nancy.Swagger.Services;
using Serilog;

namespace Fabric.IdentityProviderSearchService.Modules
{
    public class DocsModule : NancyModule
    {
        private readonly ILogger _logger;

        public DocsModule(ISwaggerMetadataProvider converter, ILogger logger) : base("/v1/docs")
        {
            _logger = logger;
            Get("/", _ => GetSwaggerUrl());
            Get("/swagger.json", _ => converter.GetSwaggerJson(Context).ToJson());
        }

        private Response GetSwaggerUrl()
        {
            _logger.Information($"getting swagger docs sitebase url: {Request.Url.SiteBase}");
            return Response.AsRedirect(
                $"{Request.Url.SiteBase}/swagger/index.html?url={Request.Url.SiteBase}/docs/swagger.json");
        }
    }
}