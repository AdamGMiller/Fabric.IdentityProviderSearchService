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

        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scopes { get; set; }
        public string[] IssuerWhiteList { get; set; }
    }
}