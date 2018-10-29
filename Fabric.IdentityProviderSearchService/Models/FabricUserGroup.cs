using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fabric.IdentityProviderSearchService.Models
{
    using Fabric.IdentityProviderSearchService.Constants;
    public class FabricUserGroup
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string SubjectId { get; set; }
        public string UserPrincipal { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string TenantId { get; set; }
        public string IdentityProvider { get; set; }
        public PrincipalType PrincipalType { get; set; }
    }
}