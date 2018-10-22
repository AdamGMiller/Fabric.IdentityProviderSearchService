using Fabric.IdentityProviderSearchService.Models;
using Microsoft.Security.Application;

namespace Fabric.IdentityProviderSearchService.Services.PrincipalQuery
{
    public class ActiveDirectoryExactMatchQuery: IPrincipalQuery
    {
        public string QueryText(string queryText, PrincipalType principalType)
        {
            var encodedSearchText = Encoder.LdapFilterEncode(queryText);
            var nameFilter = $"(|(sAMAccountName={encodedSearchText})(givenName={encodedSearchText})(sn={encodedSearchText})(cn={encodedSearchText}))";

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