﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

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