using System.Collections.Generic;
using Fabric.ActiveDirectory.Models;

namespace Fabric.ActiveDirectory.Services
{
    public interface IExternalIdentityProviderService
    {
        //TODO: make async
        ICollection<AdPrincipal> SearchUsers(string searchText);
    }
}
