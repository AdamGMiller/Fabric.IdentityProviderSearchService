using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.UI.WebControls.Expressions;
using Fabric.IdentityProviderSearchService.Constants;
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

        public async Task<IEnumerable<IFabricPrincipal>> SearchPrincipalsAsync(string searchText, string principalTypeString, string searchType = SearchTypes.Wildcard)
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

            List <IFabricPrincipal> result = new List<IFabricPrincipal>();
            foreach (var service in _externalIdentityProviderServices)
            {
                result.AddRange(await service.SearchPrincipalsAsync(searchText, principalType, searchType));
            }
            return result;
        }

        public async Task<IFabricPrincipal> FindUserBySubjectIdAsync(string subjectId)
        {
            foreach (var service in _externalIdentityProviderServices)
            {
                var subject = await service.FindUserBySubjectIdAsync(subjectId);
                if(subject != null)
                {
                    return subject;
                }
            }

            return null;
        }
    }
}