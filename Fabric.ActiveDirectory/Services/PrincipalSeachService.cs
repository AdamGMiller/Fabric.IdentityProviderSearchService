using System;
using System.Collections.Generic;
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

        public IEnumerable<AdPrincipal> SearchPrincipals(string searchText, string principalTypeString)
        {
            //set principal type based on string 
            PrincipalType principalType;
            if (string.IsNullOrEmpty(principalTypeString))
            {
                principalType = PrincipalType.UserAndGroup;
            }
            else if (principalTypeString.ToLowerInvariant().Equals("user"))
            {
                principalType = PrincipalType.User;
            }
            else if (principalTypeString.ToLowerInvariant().Equals("group"))
            {
                principalType = PrincipalType.Group;
            }
            else
            {
                //TODO: replace with custom exception
                throw new Exception("invalid principal type provided");
            }
            
            return _externalIdentityProviderService.SearchPrincipals(searchText, principalType);
        }
    }
}