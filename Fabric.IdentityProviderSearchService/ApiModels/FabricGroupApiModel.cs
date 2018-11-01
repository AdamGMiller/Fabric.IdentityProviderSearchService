namespace Fabric.IdentityProviderSearchService.ApiModels
{
    using Fabric.IdentityProviderSearchService.Constants;
    public class FabricGroupApiModel
    {
        public string ExternalIdentifier { get; set; }
        public string GroupName { get; set; }
        public string TenantId { get; set; }
        public string IdentityProvider { get; set; }
        public PrincipalType PrincipalType { get; set; }
    }
}