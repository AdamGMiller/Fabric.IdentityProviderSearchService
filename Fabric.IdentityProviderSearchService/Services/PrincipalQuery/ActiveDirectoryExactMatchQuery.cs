namespace Fabric.IdentityProviderSearchService.Services.PrincipalQuery
{
    public class ActiveDirectoryExactMatchQuery : ActiveDirectoryQuery
    {
        public override string GetFilter(string encodedSearchText)
        {
            return encodedSearchText;
        }
    }
}