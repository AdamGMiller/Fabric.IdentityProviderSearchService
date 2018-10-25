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

        public async Task<IEnumerable<T>> SearchPrincipalsAsync<T>(string searchText, string principalTypeString, string searchType, string identityProvider = "")
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

            var result = new List<T>();
            foreach (var service in _externalIdentityProviderServices)
            {
                if (identityProvider == "windows")
                {
                    result.AddRange(await service.SearchPrincipalsAsync<T>(searchText, principalType, searchType, identityProvider));
                }
                else
                {
                    result.AddRange(await service.SearchPrincipalsAsync<T>(searchText, principalType, searchType, identityProvider));
                }
            }
            return result;
        }

        public async Task<IEnumerable<IFabricGroup>> SearchGroupsAsync(string searchText, string principalTypeString, string searchType, string identityProvider)
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

            List<IFabricGroup> result = new List<IFabricGroup>();
            foreach (var service in _externalIdentityProviderServices)
            {
                result.AddRange(await service.SearchPrincipalsAsync<IFabricGroup>(searchText, principalType, searchType, identityProvider));
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