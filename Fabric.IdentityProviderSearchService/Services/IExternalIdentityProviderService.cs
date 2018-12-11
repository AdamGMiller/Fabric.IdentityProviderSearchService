using System.Collections.Generic;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Constants;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IExternalIdentityProviderService
    {
        Task<IEnumerable<IFabricPrincipal>> SearchPrincipalsAsync(string searchText, PrincipalType principalType, string searchType, string tenantId = null);
        Task<IFabricPrincipal> FindUserBySubjectIdAsync(string subjectId, string tenantId = null);
        Task<IEnumerable<IFabricGroup>> SearchGroupsAsync(string searchText, string searchType, string tenantId = null);
    }
}
