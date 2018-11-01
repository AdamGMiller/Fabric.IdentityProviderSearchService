namespace Fabric.IdentityProviderSearchService.Models
{
    using Constants;
    public class FabricGroup : IFabricGroup
    {
        public string ExternalIdentifier { get; set; }
        public string GroupName { get; set; }
        public string TenantId { get; set; }
        public string IdentityProvider { get; set; }
        public PrincipalType PrincipalType { get; set; }
    }
}