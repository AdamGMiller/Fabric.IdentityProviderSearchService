using Fabric.IdentityProviderSearchService.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class DecryptionService
    {
        public static readonly string EncryptionPrefix = "!!enc!!:";
        private readonly ICertificateService _certificateService;
        public DecryptionService(ICertificateService certificateService)
        {
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
        }

        private bool IsEncrypted(string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith(EncryptionPrefix);
        }

        public string DecryptString(string encryptedString, EncryptionCertificateSettings certificateSettings)
        {
            if (!IsEncrypted(encryptedString)) return encryptedString;

            var cert =
                _certificateService.GetCertificate(certificateSettings);
            var encryptedPasswordAsBytes =
                Convert.FromBase64String(
                    encryptedString.Replace(EncryptionPrefix, string.Empty));
            var decryptedPasswordAsBytes = cert.GetRSAPrivateKey()
                .Decrypt(encryptedPasswordAsBytes, RSAEncryptionPadding.OaepSHA1);
            return System.Text.Encoding.UTF8.GetString(decryptedPasswordAsBytes);
        }
    }
}