using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services.PrincipalQuery
{
    public class AzureExactMatchQuery: IPrincipalQuery
    {
        // TODO: switch this to be exact match
        public string QueryText(string queryText, PrincipalType principalType)
        {
            switch (principalType)
            {
                case PrincipalType.User:
                    return $"startswith(DisplayName, '{queryText}') or startswith(GivenName, '{queryText}') or startswith(UserPrincipalName, '{queryText}')";
                case PrincipalType.Group:
                    return $"startswith(DisplayName, '{queryText}')";
                default:
                    return $"(&(|(&(objectClass=user)(objectCategory=person))(objectCategory=group)){queryText})";
            }
        }
    }
}