using System.Collections.Generic;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public interface IMicrosoftGraphApi
    {
        Task<IEnumerable<FabricGraphApiGroup>> GetGroupCollectionsAsync(string filterQuery, string tenantId = null);
        Task<FabricGraphApiUser> GetUserAsync(string subjectId, string tenantId = null);
        Task<IEnumerable<FabricGraphApiUser>> GetUserCollectionsAsync(string filterQuery, string tenantId = null);
    }
}