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
    public sealed class PrincipalsModule : NancyModule
    {
        private readonly PrincipalSeachService _seachService;

        public PrincipalsModule(PrincipalSeachService seachService) : base("/principals")
        {
            _seachService = seachService;

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

                var users = _seachService.SearchPrincipals(searchRequest.SearchText);

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
                return CreateFailureResponse<AdPrincipal>(e.Message, HttpStatusCode.BadRequest);
            }
        }

        private Negotiator CreateFailureResponse<T>(string message, HttpStatusCode statusCode)
        {
            var error = ErrorFactory.CreateError<T>(message, statusCode);
            return Negotiate.WithModel(error).WithStatusCode(statusCode);
        }
    }
}
