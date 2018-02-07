using System.Collections.Generic;
using System.Threading.Tasks;
using Fabric.ActiveDirectory.Models;

namespace Fabric.ActiveDirectory.Services
{
    public interface IExternalIdentityProviderService
    {
        IEnumerable<AdPrincipal> SearchPrincipals(string searchText);
    }
}
