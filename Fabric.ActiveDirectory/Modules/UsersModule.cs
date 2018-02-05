using System;
using System.Linq;
using System.Threading.Tasks;
using Fabric.ActiveDirectory.ApiModels;
using Fabric.ActiveDirectory.Exceptions;
using Fabric.ActiveDirectory.Models;
using Fabric.ActiveDirectory.Services;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;

namespace Fabric.ActiveDirectory.Modules
{
    public class UsersModule : NancyModule
    {
        public UsersModule() : base("/users")
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
                //TODO: DI!!
                var idpResolver = new ExternalIdentityProviderServiceResolver();

                idpResolver.GetExternalIdentityProviderService(searchRequest.IdentityProvider);

                var users = new ActiveDirectoryProviderService().SearchUsers(searchRequest.SearchText);

                return users.Select(u => new UserApiModel
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    SubjectId = u.SubjectId
                });
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
