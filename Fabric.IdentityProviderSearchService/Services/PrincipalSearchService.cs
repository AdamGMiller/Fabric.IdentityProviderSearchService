using System.Collections.Generic;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Constants;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class PrincipalSearchService
    {
        private readonly IEnumerable<IExternalIdentityProviderService> _externalIdentityProviderServices;

        public PrincipalSearchService(IEnumerable<IExternalIdentityProviderService> externalIdentityProviderService)
        {
            _externalIdentityProviderServices = externalIdentityProviderService;
        }

        public async Task<IEnumerable<IFabricPrincipal>> SearchPrincipalsAsync(string searchText, string principalTypeString, string searchType, string tenantId = null)
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

            var result = new List<IFabricPrincipal>();
            foreach (var service in _externalIdentityProviderServices)
            {
               result.AddRange(await service.SearchPrincipalsAsync(searchText, principalType, searchType, tenantId).ConfigureAwait(false));
            }
            return result;
        }

        public async Task<IFabricPrincipal> FindUserBySubjectIdAsync(string subjectId, string tenantId)
        {
            foreach (var service in _externalIdentityProviderServices)
            {
                var subject = await service.FindUserBySubjectIdAsync(subjectId, tenantId);
                if(subject != null)
                {
                    return subject;
                }
            }

            return null;
        }

        public async Task<IEnumerable<IFabricGroup>> SearchGroupsAsync(string searchText, string searchType, string tenantId = null)
        {
            var groups = new List<IFabricGroup>();
            foreach (var service in _externalIdentityProviderServices)
            {
                groups.AddRange(await service.SearchGroupsAsync(searchText, searchType, tenantId).ConfigureAwait(false));
            }
            return groups;
        }
    }
}