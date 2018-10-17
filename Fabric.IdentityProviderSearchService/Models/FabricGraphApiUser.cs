using Microsoft.Graph;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class FabricGraphApiUser
    {
        public User User { get; set; }

        public FabricGraphApiUser(User user)
        {
            this.User = user;
        }

        public string TenantId { get; set; }
    }
}