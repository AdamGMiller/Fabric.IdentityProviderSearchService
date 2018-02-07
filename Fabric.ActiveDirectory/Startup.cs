using Microsoft.Owin.Extensions;
using Owin;

namespace Fabric.ActiveDirectory
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy(opt => opt.Bootstrapper = new Bootstrapper());
            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}