using System;

namespace Fabric.ActiveDirectory.Services
{
    public class ExternalIdentityProviderServiceResolver : IExternalIdentityProviderServiceResolver
    {
        private readonly string _domainName;
        private readonly IServiceProvider _serviceProvider;
        public ExternalIdentityProviderServiceResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ExternalIdentityProviderServiceResolver(string domainName)
        {
            _domainName = domainName;
        }

        public IExternalIdentityProviderService GetExternalIdentityProviderService(string identityProviderName)
        {
            //switch (identityProviderName)
            //{
            //    case FabricIdentityConstants.FabricExternalIdentityProviderTypes.Windows:
            //        return _serviceProvider.GetRequiredService<LdapProviderService>();
            //    default:
            //        throw new InvalidExternalIdentityProviderException(
            //            $"There is no search provider specified for the requested Identity Provider: {identityProviderName}.");
            //}

            return new ActiveDirectoryProviderService(_domainName);
        }
    }
}