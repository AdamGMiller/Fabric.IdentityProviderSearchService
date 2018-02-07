using System.Collections.Generic;

namespace Fabric.IdentityProviderSearchService.ApiModels
{
    public class IdpSearchResultApiModel
    {
        public ICollection<FabricPrincipalApiModel> Principals { get; set; }
        public int ResultCount { get; set; }
    }
}