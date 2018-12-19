using System;
using System.Runtime.Serialization;

namespace Fabric.IdentityProviderSearchService.Exceptions
{
    public class DirectorySearchException : Exception
    {
        public DirectorySearchException()
        {
        }

        public DirectorySearchException(string message) : base(message)
        {
        }

        public DirectorySearchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DirectorySearchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}