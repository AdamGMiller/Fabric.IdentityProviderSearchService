using IdentityModel.Client;
using System.Threading.Tasks;

namespace Fabric.IdentityProviderSearchService.Services.Azure
{
    public interface IAzureActiveDirectoryClientCredentialsService
    {
        Task<TokenResponse> GetAzureAccessTokenAsync(string tenantId);
    }
}