namespace Fabric.IdentityProviderSearchService.Models
{
    public class SearchRequest
    {
        public string IdentityProvider { get; set; }
        public string SearchText { get; set; }        
        public string Type { get; set; }
        public string Tenant { get; set; }
    }
}