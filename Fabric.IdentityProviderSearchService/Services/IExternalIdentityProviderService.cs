using System.Collections.Generic;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IExternalIdentityProviderService
    {
        IEnumerable<FabricPrincipal> SearchPrincipals(string searchText, PrincipalType principalType);
        FabricPrincipal FindUserBySubjectId(string subjectId);
    }
}
