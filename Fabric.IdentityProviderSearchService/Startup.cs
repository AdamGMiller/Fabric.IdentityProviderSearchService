using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Logging;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Owin.Extensions;
using Owin;
using Serilog.Core;

namespace Fabric.IdentityProviderSearchService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Add(new WebConfigProvider())
                .Build();

            var appConfig = new AppConfiguration();
            ConfigurationBinder.Bind(configuration, appConfig);

            var logger = LogFactory.CreateTraceLogger(new LoggingLevelSwitch(), appConfig.ApplicationInsights);
            logger.Information("IdentityProviderSearchService is starting up...");

            appConfig.ConfigureIdentityServiceUrl();

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = appConfig.IdentityServerConfidentialClientSettings.Authority,
                RequiredScopes = appConfig.IdentityServerConfidentialClientSettings.Scopes
            });
            
            app.UseNancy(opt => opt.Bootstrapper = new Bootstrapper(appConfig, logger));
            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}