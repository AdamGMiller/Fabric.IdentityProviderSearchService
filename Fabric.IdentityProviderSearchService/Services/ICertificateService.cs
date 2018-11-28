using Fabric.IdentityProviderSearchService.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace Fabric.IdentityProviderSearchService.Services
{
    public interface ICertificateService
    {
        X509Certificate2 GetCertificate(EncryptionCertificateSettings settings);
    }
}
