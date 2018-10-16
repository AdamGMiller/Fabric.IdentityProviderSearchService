using Fabric.IdentityProviderSearchService.Models;
using System.Threading.Tasks;

namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public interface IAzureActiveDirectoryClientCredentialsService
    {
        Task<AzureActiveDirectoryResponse> GetAzureAccessTokenAsync(string tenantId);
    }
}