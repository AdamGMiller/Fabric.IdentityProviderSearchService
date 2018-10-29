﻿namespace Fabric.IdentityProviderSearchService.ApiModels
{
    public class FabricPrincipalApiModel
    {
        public string SubjectId { get; set; }       
        public string UserPrincipal { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string IdentityProvider { get; set; }
        public string PrincipalType { get; set; }        
    }
}