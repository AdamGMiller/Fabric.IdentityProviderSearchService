using System.Collections.Generic;
using System.Linq;
using Fabric.ActiveDirectory.ApiModels;
using Fabric.ActiveDirectory.Exceptions;
using Fabric.ActiveDirectory.Models;
using Fabric.ActiveDirectory.Services;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;

namespace Fabric.ActiveDirectory.Modules
{
    public class PrincipalsModule : NancyModule
    {
        public PrincipalsModule() : base("/principals")
        {
            Get("/search",
                _ => Search(),
                null,
                "Search");
        }

        private dynamic Search()
        {
            //TODO: make async
            var searchRequest = this.Bind<SearchRequest>();

            try
            {
                var principals = new List<AdPrincipalApiModel>();
                //TODO: DI!!
                var idpResolver = new ExternalIdentityProviderServiceResolver("hqcatalyst");

                var service = idpResolver.GetExternalIdentityProviderService(searchRequest.IdentityProvider);

                var users = service.SearchUsers(searchRequest.SearchText);

                principals.AddRange(users.Select(u => new AdPrincipalApiModel
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    SubjectId = u.SubjectId,
                    PrincipalType = u.PrincipalType.ToString()
                }));

                return new AdSearchResultApiModel
                {
                    Principals = principals,
                    ResultCount = principals.Count
                };
            }
            catch (InvalidExternalIdentityProviderException e)
            {
                return CreateFailureResponse(e.Message, HttpStatusCode.BadRequest);
            }
            
        }

        private Negotiator CreateFailureResponse(string message, HttpStatusCode statusCode)
        {
            //TODO: create a better Error object to pass to WithModel()
            return Negotiate.WithModel(new {Message = message}).WithStatusCode(statusCode);
        }
    }
}
