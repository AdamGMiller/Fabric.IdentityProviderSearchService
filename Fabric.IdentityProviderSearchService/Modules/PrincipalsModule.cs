﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Fabric.IdentityProviderSearchService.ApiModels;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;

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
            this.RequiresClaims(SearchPrincipalClaim);
            var searchRequest = this.Bind<SearchRequest>();

            if (string.IsNullOrEmpty(searchRequest.SearchText))
            {
                return CreateFailureResponse<FabricPrincipalApiModel>("Search text was not provided and is required",
                    HttpStatusCode.BadRequest);
            }

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
                return CreateFailureResponse<FabricPrincipalApiModel>(e.Message, HttpStatusCode.BadRequest);
            }
            catch (BadRequestException e)
            {
                return CreateFailureResponse<FabricPrincipalApiModel>(e.Message, HttpStatusCode.BadRequest);
            }
        }

        private Negotiator CreateFailureResponse<T>(string message, HttpStatusCode statusCode)
        {
            var error = ErrorFactory.CreateError<T>(message, statusCode);
            return Negotiate.WithModel(error).WithStatusCode(statusCode);
        }

        private Predicate<Claim> SearchPrincipalClaim
        {
            get { return claim => claim.Type == "scope" && claim.Value == Scopes.SearchPrincipalsScope; }
        }
    }
}
