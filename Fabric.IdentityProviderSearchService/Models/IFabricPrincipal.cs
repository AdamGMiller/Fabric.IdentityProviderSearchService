namespace Fabric.IdentityProviderSearchService.Models
{
    using Constants;
    public interface IFabricPrincipal
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string MiddleName { get; set; }
        string SubjectId { get; set; }
        string UserPrincipal { get; set; }
        string UniqueId { get; set; }
        string DisplayName { get; set; }
        string IdentityProvider { get; set; }
        string TenantId { get; set; }
        PrincipalType PrincipalType { get; set; }
    }
}
