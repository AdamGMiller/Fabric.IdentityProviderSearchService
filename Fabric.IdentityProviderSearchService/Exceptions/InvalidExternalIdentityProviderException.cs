using System;

namespace Fabric.IdentityProviderSearchService.Exceptions
{
    public class InvalidExternalIdentityProviderException : Exception
    {
        public InvalidExternalIdentityProviderException()
        {
        }

        public InvalidExternalIdentityProviderException(string message) : base(message)
        {
        }

        public InvalidExternalIdentityProviderException(string message, Exception innerException) : base(message,
            innerException)
        {
        }
    }
}