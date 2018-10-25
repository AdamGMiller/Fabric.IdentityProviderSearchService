using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class SearchGroupRequest
    {
        public string IdentityProvider { get; set; }
        public string GroupName { get; set; }
        public string SearchText { get; set; }
        public string Type { get; set; }
        public string Tenant { get; set; }
    }
}