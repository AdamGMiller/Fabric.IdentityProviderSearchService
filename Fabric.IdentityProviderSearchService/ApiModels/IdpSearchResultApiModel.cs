using System.Collections.Generic;

namespace Fabric.IdentityProviderSearchService.ApiModels
{
    public class IdpSearchResultApiModel<T>
    {
        public ICollection<T> Principals { get; set; }
        public int ResultCount { get; set; }
    }
}