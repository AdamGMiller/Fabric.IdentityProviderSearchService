namespace Fabric.IdentityProviderSearchService.Models
{
    public class AdPrincipal
    {       
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string SubjectId { get; set; }
        public PrincipalType PrincipalType { get; set; }

    }

    public enum PrincipalType
    {
        User,
        Group,
        UserAndGroup
    }
}