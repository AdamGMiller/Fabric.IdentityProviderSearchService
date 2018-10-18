using System.Collections.Generic;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class PrincipalSearchService
    {
        private readonly IEnumerable<IExternalIdentityProviderService> _externalIdentityProviderServices;

        public PrincipalSearchService(IEnumerable<IExternalIdentityProviderService> externalIdentityProviderService)
        {
            _externalIdentityProviderServices = externalIdentityProviderService;
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

            List<IFabricPrincipal> result = new List<IFabricPrincipal>();
            foreach (var service in _externalIdentityProviderServices)
            {
                result.AddRange(service.SearchPrincipals(searchText, principalType));
            }
            return result;
        }

        public IFabricPrincipal FindUserBySubjectId(string subjectId)
        {
            foreach (var service in _externalIdentityProviderServices)
            {
                var subject = service.FindUserBySubjectId(subjectId);
                if(subject != null)
                {
                    return subject;
                }
            }

            return null;
        }
    }
}