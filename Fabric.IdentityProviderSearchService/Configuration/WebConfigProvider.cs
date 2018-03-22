using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Fabric.IdentityProviderSearchService.Configuration
{
    public class WebConfigProvider : ConfigurationProvider, IConfigurationSource
    {
        public override void Load()
        {
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                Data.Add(key, ConfigurationManager.AppSettings[key]);
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}