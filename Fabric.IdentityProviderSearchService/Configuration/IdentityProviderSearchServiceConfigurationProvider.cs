using Fabric.IdentityProviderSearchService.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fabric.IdentityProviderSearchService.Configuration
{
    public class IdentityProviderSearchServiceConfigurationProvider
    {
        private ICertificateService _certificateService;
        private DecryptionService _decryptionService;
        private EncryptionCertificateSettings _encryptionCertificateSettings;

        public IdentityProviderSearchServiceConfigurationProvider(EncryptionCertificateSettings encryptionCertificateSettings, ICertificateService certificateService, DecryptionService decryptionService)
        {
            _encryptionCertificateSettings = encryptionCertificateSettings ?? throw new ArgumentNullException(nameof(encryptionCertificateSettings));
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
            _decryptionService = decryptionService ?? throw new ArgumentNullException(nameof(decryptionService));
        }

        public IAppConfiguration GetAppConfiguration(IAppConfiguration appConfig)
        {
            DecryptEncryptedValues(appConfig);
            return appConfig;
        }

        private void DecryptEncryptedValues(IAppConfiguration appConfig)
        {
            if(appConfig.EncryptionCertificateSettings != null)
            {
                foreach (var setting in appConfig.AzureActiveDirectoryClientSettings.ClientAppSettings)
                {
                    setting.ClientSecret = _decryptionService.DecryptString(setting.ClientSecret, _encryptionCertificateSettings);
                }
            }
        }
    }
}