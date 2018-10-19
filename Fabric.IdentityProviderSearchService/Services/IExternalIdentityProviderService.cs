using System.Collections.Generic;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IExternalIdentityProviderService
    {
        Task<IEnumerable<IFabricPrincipal>> SearchPrincipalsAsync(string searchText, PrincipalType principalType);
        Task<IFabricPrincipal> FindUserBySubjectIdAsync(string subjectId);
    }
}
