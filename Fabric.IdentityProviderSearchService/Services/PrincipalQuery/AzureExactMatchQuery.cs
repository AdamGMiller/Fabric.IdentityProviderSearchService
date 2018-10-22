using System;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services.PrincipalQuery
{
    public class AzureExactMatchQuery : IPrincipalQuery
    {
        // TODO: switch this to be exact match
        public string QueryText(string searchText, PrincipalType principalType)
        {
            switch (principalType)
            {
                case PrincipalType.User:
                    return
                        $"startswith(DisplayName, '{searchText}') or startswith(GivenName, '{searchText}') or startswith(UserPrincipalName, '{searchText}')";
                case PrincipalType.Group:
                    return $"startswith(DisplayName, '{searchText}')";
                default:
                    throw new Exception($"Query type {principalType} not supported in Azure AD.");
            }
        }
    }
}