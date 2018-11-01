namespace Fabric.IdentityProviderSearchService.Models
{
    using Constants;
    public class FabricPrincipal : IFabricPrincipal
    {       
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string SubjectId { get; set; }
        public string UserPrincipal { get; set; }
        public string ExternalIdentifier { get; set; }
        public string DisplayName { get; set; }
        public string IdentityProvider { get; set; }
        public string TenantId { get; set; }
        public PrincipalType PrincipalType { get; set; }
    }
}