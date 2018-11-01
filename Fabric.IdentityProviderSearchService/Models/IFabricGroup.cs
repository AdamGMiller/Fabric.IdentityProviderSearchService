namespace Fabric.IdentityProviderSearchService.Models
{
    using Constants;
    public interface IFabricGroup
    {
        string ExternalIdentifier { get; set; }
        string GroupName { get; set; }
        string TenantId { get; set; }
        string IdentityProvider { get; set; }
        PrincipalType PrincipalType { get; set; }
    }
}
