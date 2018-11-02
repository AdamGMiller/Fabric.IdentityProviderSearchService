﻿namespace Fabric.IdentityProviderSearchService.Models
{
    public class SearchGroupRequest
    {
        public string IdentityProvider { get; set; }
        public string GroupName { get; set; }
        public string Type { get; set; }
        public string TenantId { get; set; }
    }
}