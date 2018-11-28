using System;

namespace Fabric.IdentityProviderSearchService.Exceptions
{
    public class IdentityConfigurationException : Exception
    {
        public IdentityConfigurationException()
        {
        }

        public IdentityConfigurationException(string message) : base(message)
        {
        }

        public IdentityConfigurationException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}