using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Logging;
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
                .Build();

            var appConfig = new AppConfiguration();
            ConfigurationBinder.Bind(configuration, appConfig);

            var logger = LogFactory.CreateTraceLogger(new LoggingLevelSwitch(), appConfig.ApplicationInsights);
                        
            app.UseNancy(opt => opt.Bootstrapper = new Bootstrapper(appConfig, logger));
            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}