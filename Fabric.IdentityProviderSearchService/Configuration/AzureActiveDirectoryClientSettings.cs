using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fabric.IdentityProviderSearchService.Configuration
{
    public class AzureActiveDirectoryClientSettings
    {
        public AzureActiveDirectoryClientSettings()
        {

        }

        public AzureClientApplicationSettings[] ClientAppSettings { get; set; }

        public string Authority { get; set; }
    }
}