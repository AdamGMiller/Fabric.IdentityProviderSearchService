using Fabric.IdentityProviderSearchService.Models;
using Microsoft.Security.Application;

namespace Fabric.IdentityProviderSearchService.Services.PrincipalQuery
{
    public abstract class ActiveDirectoryQuery : IPrincipalQuery
    {
        public abstract string GetFilter(string encodedSearchText);

        public virtual string QueryText(string searchText, PrincipalType principalType)
        {
            var encodedSearchText = Encoder.LdapFilterEncode(searchText);
            var filter = GetFilter(encodedSearchText);
            var nameFilter = $"(|(sAMAccountName={filter})(givenName={filter})(sn={filter})(cn={filter}))";
            return GetCategoryFilter(nameFilter, principalType);
        }

        protected virtual string GetCategoryFilter(string nameFilter, PrincipalType principalType)
        {
            switch (principalType)
            {
                case PrincipalType.User:
                    return $"(&(objectClass=user)(objectCategory=person){nameFilter})";
                case PrincipalType.Group:
                    return $"(&(objectCategory=group){nameFilter})";
                default:
                    return $"(&(|(&(objectClass=user)(objectCategory=person))(objectCategory=group)){nameFilter})";
            }
        }
    }
}