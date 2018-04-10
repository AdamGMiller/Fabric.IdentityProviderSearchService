using System;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Infrastructure.PipelineHooks;
using Fabric.IdentityProviderSearchService.Services;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Conventions;
using Nancy.Responses.Negotiation;
using Nancy.Swagger.Services;
using Nancy.TinyIoc;
using Serilog;
using Swagger.ObjectModel;
using Swagger.ObjectModel.Builders;

namespace Fabric.IdentityProviderSearchService
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly ILogger _logger;
        private readonly IAppConfiguration _appConfig;

        public Bootstrapper(IAppConfiguration appConfig, ILogger logger)
        {
            _logger = logger;
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            InitializeSwaggerMetadata();

            pipelines.OnError.AddItemToEndOfPipeline(
                (ctx, ex) => new OnErrorHooks(_logger)
                    .HandleInternalServerError(
                        ctx,
                        ex,
                        container.Resolve<IResponseNegotiator>(),
                        true)); //TODO: find out how to determine if IsDevelopment is true

            pipelines.BeforeRequest += ctx => RequestHooks.RemoveContentTypeHeaderForGet(ctx);
            pipelines.BeforeRequest += ctx => RequestHooks.SetDefaultVersionInUrl(ctx);

            pipelines.AfterRequest += ctx =>
            {
                foreach (var corsHeader in HttpResponseHeaders.CorsHeaders)
                {
                    ctx.Response.Headers.Add(corsHeader.Item1, corsHeader.Item2);
                }
            };

            container.Register(_appConfig);
            container.Register(_logger);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
            container.Register<IActiveDirectoryProxy, ActiveDirectoryProxy>();
            container.Register<IExternalIdentityProviderService, ActiveDirectoryProviderService>();
            container.Register<PrincipalSearchService, PrincipalSearchService>();
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/swagger/ui"));
        }

        public override void Configure(INancyEnvironment environment)
        {
            base.Configure(environment);
            environment.Tracing(true, true);
        }

        private void InitializeSwaggerMetadata()
        {
            SwaggerMetadataProvider.SetInfo("Fabric Identity Provider Search Service", "v1",
                "Fabric.IdentityProviderSearchService provides an API for searching identity providers.");

            var securitySchemeBuilder = new Oauth2SecuritySchemeBuilder();
            securitySchemeBuilder.Flow(Oauth2Flows.Implicit);
            securitySchemeBuilder.Description("Authentication with Fabric.Identity");
            securitySchemeBuilder.AuthorizationUrl(@"http://localhost:5001");
            securitySchemeBuilder.Scope(Scopes.SearchPrincipalsScope, "Grants access to search api");
            try
            {
                SwaggerMetadataProvider.SetSecuritySchemeBuilder(securitySchemeBuilder, "fabric.identityprovidersearchservice");
            }
            catch (ArgumentException ex)
            {
                _logger.Warning("Error configuring Swagger Security Scheme. {exceptionMessage}", ex.Message);
            }
            catch (NullReferenceException ex)
            {
                _logger.Warning("Error configuring Swagger Security Scheme: {exceptionMessage", ex.Message);
            }
        }
    }
}