using IdentityModel.Client;
using System;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class TokenResponseWrapper
    {
        public DateTime ExpiryTime { get; set; }
        public TokenResponse Response { get; set; }
    }
}