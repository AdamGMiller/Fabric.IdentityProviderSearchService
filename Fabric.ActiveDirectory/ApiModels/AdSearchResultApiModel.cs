using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fabric.ActiveDirectory.ApiModels
{
    public class AdSearchResultApiModel
    {
        public ICollection<AdPrincipalApiModel> Principals { get; set; }
        public int ResultCount { get; set; }
    }
}