using System;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;

namespace Fabric.IdentityProviderSearchService.Services.PrincipalQuery
{
    public class AzureExactMatchQuery : IAzureQuery
    {
        public string QueryText(string searchText, PrincipalType principalType)
        {
            switch (principalType)
            {
                case PrincipalType.User:
                    return
                        $"DisplayName eq '{searchText}' or GivenName eq '{searchText}' or UserPrincipalName eq '{searchText}' or Surname eq '{searchText}'";
                case PrincipalType.Group:
                    return $"DisplayName eq '{searchText}'";
                default:
                    throw new DirectorySearchException($"Query type {principalType} not supported in Azure AD.");
            }
        }
    }
}