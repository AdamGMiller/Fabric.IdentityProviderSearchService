using System;

namespace Fabric.IdentityProviderSearchService.Exceptions
{
    public class AzureActiveDirectoryException : Exception
    {
        public AzureActiveDirectoryException()
        {
        }

        public AzureActiveDirectoryException(string message) : base(message)
        {
        }

        public AzureActiveDirectoryException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}