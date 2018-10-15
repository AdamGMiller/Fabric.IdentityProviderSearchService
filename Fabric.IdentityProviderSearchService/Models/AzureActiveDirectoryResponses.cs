using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class AzureActiveDirectoryResponse
    {
        public string access_token { get; set; }

        public string TokenType { get; set; }

        public string ExpiresIn { get; set; }
    }
}