namespace Fabric.IdentityProviderSearchService.Models
{
    using Fabric.IdentityProviderSearchService.Constants;
    public interface IFabricGroup
    {
        string GroupId { get; set; }
        string GroupFirstName { get; set; }
        string TenantId { get; set; }
        PrincipalType PrincipalType { get; set; }
    }
}
