using System.Collections.Generic;
using System.Linq;
using Fabric.IdentityProviderSearchService.ApiModels;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;

namespace Fabric.IdentityProviderSearchService.Modules
{
    public sealed class PrincipalsModule : NancyModule
    {
        private readonly PrincipalSeachService _seachService;

        public PrincipalsModule(PrincipalSeachService seachService) : base("/v1/principals")
        {
            _seachService = seachService;

            Get("/search",
                _ => Search(),
                null,
                "Search");
        }

        private dynamic Search()
        {            
            var searchRequest = this.Bind<SearchRequest>();

            try
            {
                var principals = new List<FabricPrincipalApiModel>();

                var users = _seachService.SearchPrincipals(searchRequest.SearchText, searchRequest.Type);

                principals.AddRange(users.Select(u => new FabricPrincipalApiModel
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    SubjectId = u.SubjectId,
                    PrincipalType = u.PrincipalType.ToString().ToLower()
                }));

                return new IdpSearchResultApiModel
                {
                    Principals = principals,
                    ResultCount = principals.Count
                };
            }
            catch (InvalidExternalIdentityProviderException e)
            {
                return CreateFailureResponse<FabricPrincipal>(e.Message, HttpStatusCode.BadRequest);
            }
            catch (BadRequestException e)
            {
                return CreateFailureResponse<FabricPrincipal>(e.Message, HttpStatusCode.BadRequest);
            }
        }

        private Negotiator CreateFailureResponse<T>(string message, HttpStatusCode statusCode)
        {
            var error = ErrorFactory.CreateError<T>(message, statusCode);
            return Negotiate.WithModel(error).WithStatusCode(statusCode);
        }
    }
}
