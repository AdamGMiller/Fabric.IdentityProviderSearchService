using Microsoft.Graph;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class FabricGraphApiGroup
    {
        public FabricGraphApiGroup(Group group)
        {
            Group = group;
        }

        public Group Group { get; set; }

        public string IdentityProvider { get; set; }

        public string TenantId { get; set; }
    }
}