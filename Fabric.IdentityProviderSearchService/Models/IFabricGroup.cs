namespace Fabric.IdentityProviderSearchService.Models
{
    using Fabric.IdentityProviderSearchService.Constants;
    public interface IFabricGroup
    {
        string GroupId { get; set; }
        string GroupName { get; set; }
        string TenantId { get; set; }
        PrincipalType PrincipalType { get; set; }
    }
}
