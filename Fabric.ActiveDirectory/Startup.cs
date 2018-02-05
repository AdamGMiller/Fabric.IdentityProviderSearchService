using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Extensions;
using Owin;

namespace Fabric.ActiveDirectory
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy();
            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}