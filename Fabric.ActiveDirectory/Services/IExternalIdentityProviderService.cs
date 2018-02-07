using System.Collections.Generic;
using Fabric.ActiveDirectory.Models;

namespace Fabric.ActiveDirectory.Services
{
    public interface IExternalIdentityProviderService
    {
        IEnumerable<AdPrincipal> SearchPrincipals(string searchText, PrincipalType principalType);
    }
}
