using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fabric.IdentityProviderSearchService.Models
{
    using Fabric.IdentityProviderSearchService.Constants;
    public interface IFabricUserGroup
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string MiddleName { get; set; }
        string SubjectId { get; set; }
        string UserPrincipal { get; set; }
        string GroupId { get; set; }
        string GroupName { get; set; }
        string TenantId { get; set; }
        string IdentityProvider { get; set; }
        PrincipalType PrincipalType { get; set; }
    }
}