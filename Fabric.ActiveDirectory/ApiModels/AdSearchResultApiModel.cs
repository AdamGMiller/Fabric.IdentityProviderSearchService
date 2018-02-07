using System.Collections.Generic;

namespace Fabric.IdentityProviderSearchService.ApiModels
{
    public class AdSearchResultApiModel
    {
        public ICollection<AdPrincipalApiModel> Principals { get; set; }
        public int ResultCount { get; set; }
    }
}