using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IMicrosoftGraphApi
    {
        Task<IEnumerable<IGraphServiceGroupsCollectionPage>> GetGroupCollectionsAsync(string filterQuery);
        Task<User> GetUserAsync(string subjectId);
        Task<IEnumerable<IGraphServiceUsersCollectionPage>> GetUserCollectionsAsync(string filterQuery);
    }
}