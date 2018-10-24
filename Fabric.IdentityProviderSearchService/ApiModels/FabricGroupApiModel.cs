namespace Fabric.IdentityProviderSearchService.ApiModels
{
    using Fabric.IdentityProviderSearchService.Constants;
    public class FabricGroupApiModel
    {
        public string GroupId { get; set; }
        public string FirstName { get; set; }
        public string TenantId { get; set; }
        public PrincipalType PrincipalType { get; set; }
    }
}