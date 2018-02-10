namespace Fabric.IdentityProviderSearchService.Models
{
    public interface IFabricPrincipal
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string MiddleName { get; set; }
        string SubjectId { get; set; }
        PrincipalType PrincipalType { get; set; }
    }
}
