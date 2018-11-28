using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Fabric.IdentityProviderSearchService.Configuration;
using Fabric.IdentityProviderSearchService.Exceptions;
using IdentityModel;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class WindowsCertificateService : ICertificateService
    {
        public X509Certificate2 GetEncryptionCertificate(EncryptionCertificateSettings settings)
        {
            if (string.IsNullOrEmpty(settings?.EncryptionCertificateThumbprint))
            {
                throw new IdentityConfigurationException("No certificate defined in configuration for EncryptionCertificateSettings.EncryptionCertificateThumbprint, encrypted value cannot be decrypted");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new IdentityConfigurationException("Do not encrypt settings when running on a Linux container, instead use Docker Secrets to protect sensitive configuration settings.");
            }
            var cert = GetCertificateByThumbprint(settings.EncryptionCertificateThumbprint);
            return cert;
        }

        private X509Certificate2 GetCertificateByThumbprint(string thumbprint)
        {
            var cleanedThumbprint = CleanThumbprint(thumbprint);
            return X509.LocalMachine.My.Thumbprint.Find(cleanedThumbprint, validOnly: false).FirstOrDefault();
        }

        private string CleanThumbprint(string thumbprint)
        {
            var cleanedThumbprint = Regex.Replace(thumbprint, @"\s+", "").ToUpperInvariant();
            return cleanedThumbprint;
        }

        public RSA GetEncryptionCertificatePrivateKey(EncryptionCertificateSettings certificateSettings)
        {
            var cert = GetEncryptionCertificate(certificateSettings);
            return cert.GetRSAPrivateKey();
        }
    }
}