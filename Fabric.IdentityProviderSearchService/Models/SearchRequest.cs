namespace Fabric.IdentityProviderSearchService.Models
{
    public class SearchRequest
    {
        public string SearchText { get; set; }        
        public string Type { get; set; }
        public string IdentityProvider { get; set; }
    }
}