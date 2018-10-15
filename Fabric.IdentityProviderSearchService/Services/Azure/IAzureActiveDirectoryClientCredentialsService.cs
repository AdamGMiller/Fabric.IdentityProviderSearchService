using System.Threading.Tasks;

namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public interface IAzureActiveDirectoryClientCredentialsService
    {
        Task<string> GetAzureAccessTokenAsync(string tenantId);

        string[] GetTenants();
    }
}