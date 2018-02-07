using System.Collections.Generic;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface IExternalIdentityProviderService
    {
        IEnumerable<AdPrincipal> SearchPrincipals(string searchText, PrincipalType principalType);
        AdPrincipal FindUserBySubjectId(string subjectId);
    }
}
