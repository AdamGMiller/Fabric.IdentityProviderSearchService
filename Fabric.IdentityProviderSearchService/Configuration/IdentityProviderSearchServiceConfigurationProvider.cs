using Fabric.IdentityProviderSearchService.Services;
using System;

namespace Fabric.IdentityProviderSearchService.Configuration
{
    public class IdentityProviderSearchServiceConfigurationProvider
    {
        private DecryptionService _decryptionService;
        private EncryptionCertificateSettings _encryptionCertificateSettings;

        public IdentityProviderSearchServiceConfigurationProvider(EncryptionCertificateSettings encryptionCertificateSettings, DecryptionService decryptionService)
        {
            _encryptionCertificateSettings = encryptionCertificateSettings ?? throw new ArgumentNullException(nameof(encryptionCertificateSettings));
            _decryptionService = decryptionService ?? throw new ArgumentNullException(nameof(decryptionService));
        }

        public IAppConfiguration GetAppConfiguration(IAppConfiguration appConfig)
        {
            DecryptEncryptedValues(appConfig);
            return appConfig;
        }

        private void DecryptEncryptedValues(IAppConfiguration appConfig)
        {
            if(_encryptionCertificateSettings != null && appConfig.AzureActiveDirectoryClientSettings.ClientAppSettings != null)
            {
                foreach (var setting in appConfig.AzureActiveDirectoryClientSettings.ClientAppSettings)
                {
                    setting.ClientSecret = _decryptionService.DecryptString(setting.ClientSecret, _encryptionCertificateSettings);
                }
            }
        }
    }
}