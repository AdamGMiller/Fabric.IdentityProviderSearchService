using System.Collections.Generic;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IExternalIdentityProviderService
    {
        IEnumerable<IFabricPrincipal> SearchPrincipals(string searchText, PrincipalType principalType);
        IFabricPrincipal FindUserBySubjectId(string subjectId);
    }
}
