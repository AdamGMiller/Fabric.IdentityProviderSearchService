using System;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services.PrincipalQuery
{
    public class AzureWildcardQuery : IAzureQuery
    {
        public string QueryText(string searchText, PrincipalType principalType)
        {
            switch (principalType)
            {
                case PrincipalType.User:
                    return
                        $"startswith(DisplayName, '{searchText}') or startswith(GivenName, '{searchText}') or startswith(UserPrincipalName, '{searchText}') or startswith(Surname, '{searchText}')";
                case PrincipalType.Group:
                    return $"startswith(DisplayName, '{searchText}')";
                default:
                    throw new DirectorySearchException($"Query type {principalType} not supported in Azure AD.");
            }
        }
    }
}