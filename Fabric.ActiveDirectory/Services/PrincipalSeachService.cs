using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fabric.ActiveDirectory.Models;

namespace Fabric.ActiveDirectory.Services
{
    public class PrincipalSeachService
    {
        private readonly IExternalIdentityProviderService _externalIdentityProviderService;

        public PrincipalSeachService(IExternalIdentityProviderService externalIdentityProviderService)
        {
            _externalIdentityProviderService = externalIdentityProviderService;
        }

        public ICollection<AdPrincipal> SearchPrincipals(string searchText)
        {
            return _externalIdentityProviderService.SearchPrincipals(searchText);
        }
    }
}