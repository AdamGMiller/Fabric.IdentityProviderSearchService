using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services.PrincipalQuery
{
    public interface IPrincipalQuery
    {
        string QueryText(string searchText, PrincipalType principalType);
    }
}