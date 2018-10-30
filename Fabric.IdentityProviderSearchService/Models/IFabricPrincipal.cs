namespace Fabric.IdentityProviderSearchService.Models
{
    using Fabric.IdentityProviderSearchService.Constants;
    public interface IFabricPrincipal
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string MiddleName { get; set; }
        string SubjectId { get; set; }
        string UserPrincipal { get; set; }
        string GroupId { get; set; }
        string GroupName { get; set; }
        string IdentityProvider { get; set; }
        string TenantId { get; set; }
        PrincipalType PrincipalType { get; set; }
    }
}
