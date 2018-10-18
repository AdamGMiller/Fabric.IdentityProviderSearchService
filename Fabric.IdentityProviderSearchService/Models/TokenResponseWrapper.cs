using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class TokenResponseWrapper
    {
        public DateTime ExpiryTime { get; set; }
        public TokenResponse Response { get; set; }
    }
}