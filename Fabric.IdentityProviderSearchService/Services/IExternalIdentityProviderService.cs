using System.Collections.Generic;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Constants;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IExternalIdentityProviderService
    {
        Task<IEnumerable<T>> SearchPrincipalsAsync<T>(string searchText, PrincipalType principalType, string searchType, string identityProvider = "");
        Task<IFabricPrincipal> FindUserBySubjectIdAsync(string subjectId);
    }
}
