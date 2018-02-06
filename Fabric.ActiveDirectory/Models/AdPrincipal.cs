using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fabric.ActiveDirectory.ApiModels;

namespace Fabric.ActiveDirectory.Models
{
    public class AdPrincipal
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string SubjectId { get; set; }
        public PrincipalType PrincipalType { get; set; }

    }

    public enum PrincipalType
    {
        User,
        Group
    }
}