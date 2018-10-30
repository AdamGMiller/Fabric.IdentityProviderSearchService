﻿namespace Fabric.IdentityProviderSearchService.Models
{
    using Fabric.IdentityProviderSearchService.Constants;
    public class FabricGroup : IFabricGroup
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string TenantId { get; set; }
        public PrincipalType PrincipalType { get; set; }
    }
}