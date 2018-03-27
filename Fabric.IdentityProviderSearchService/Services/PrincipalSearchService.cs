using System.Collections.Generic;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class PrincipalSearchService
    {
        private readonly IExternalIdentityProviderService _externalIdentityProviderService;

        public PrincipalSearchService(IExternalIdentityProviderService externalIdentityProviderService)
        {
            _externalIdentityProviderService = externalIdentityProviderService;
        }

        public IEnumerable<IFabricPrincipal> SearchPrincipals(string searchText, string principalTypeString)
        {           
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
                throw new BadRequestException("invalid principal type provided. valid values are 'user' and 'group'");
            }
            
            return _externalIdentityProviderService.SearchPrincipals(searchText, principalType);
        }

        public IFabricPrincipal FindUserBySubjectId(string subjectId)
        {
            return _externalIdentityProviderService.FindUserBySubjectId(subjectId);
        }
    }
}