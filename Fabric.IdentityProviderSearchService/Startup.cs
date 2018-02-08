using Fabric.IdentityProviderSearchService.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Owin.Extensions;
using Owin;

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

            app.UseNancy(opt => opt.Bootstrapper = new Bootstrapper(appConfig));
            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}