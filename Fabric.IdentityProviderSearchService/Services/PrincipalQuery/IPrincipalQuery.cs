using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services.PrincipalQuery
{
    public interface IPrincipalQuery
    {
        string QueryText(string queryText, PrincipalType principalType);
    }
}