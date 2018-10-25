using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fabric.IdentityProviderSearchService.ApiModels;
using Fabric.IdentityProviderSearchService.Constants;
using Fabric.IdentityProviderSearchService.Exceptions;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Fabric.IdentityProviderSearchService.Services.PrincipalQuery;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using Serilog;

namespace Fabric.IdentityProviderSearchService.Modules
{
    public sealed class PrincipalsModule : NancyModule
    {
        private readonly PrincipalSearchService _searchService;
        private readonly ILogger _logger;

        public PrincipalsModule(PrincipalSearchService searchService, ILogger logger) : base("/v1/principals")
        {
            _searchService = searchService;
            _logger = logger;

            Get("/search",
                async _ => await SearchAsync().ConfigureAwait(false),
                null,
                "SearchAsync");

            Get("{idP}/search",
                async _ => await SearchByIdpAsync().ConfigureAwait(false),
                null,
                "SearchByIdpAsync");

            Get("/user",
                async _ => await SearchForUserAsync().ConfigureAwait(false),
                null,
                "SearchForUserAsync");

            Get("/{idP}/groups/{groupName}?tenant={tenant}",
                async _ => await SearchForGroupsAsync().ConfigureAwait(false),
                null,
                "SearchForGroupsAsync");
        }

        private async Task<dynamic> SearchForUserAsync()
        {
            this.RequiresClaims(SearchPrincipalClaim);
            var searchRequest = this.Bind<SubjectIdRequest>();

            if (string.IsNullOrEmpty(searchRequest.SubjectId))
            {
                return CreateFailureResponse<FabricPrincipalApiModel>("Subject Id was not provided and is required",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                _logger.Information($"searching for user with subject id: {searchRequest.SubjectId}");
                var user = await _searchService.FindUserBySubjectIdAsync(searchRequest.SubjectId);

                return new FabricPrincipalApiModel
                {
                    FirstName = user?.FirstName,
                    LastName = user?.LastName,
                    MiddleName = user?.MiddleName,
                    SubjectId = user?.SubjectId,
                    PrincipalType = user?.PrincipalType.ToString().ToLower()
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
            catch (Exception e)
            {
                return CreateFailureResponse<object>(e.Message, HttpStatusCode.BadRequest);
            }
        }

        private async Task<dynamic> SearchForGroupsAsync()
        {
            this.RequiresClaims(SearchPrincipalClaim);
            var searchRequest = this.Bind<SearchRequest>();

            if (string.IsNullOrEmpty(searchRequest.SearchText))
            {
                return CreateFailureResponse<FabricGroupApiModel>("Search text was not provided and is required",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                var principals = new List<FabricGroupApiModel>();

                string tenantInfo = "";

                if (!string.IsNullOrEmpty(searchRequest.Tenant))
                {
                    tenantInfo = ($", Tenant={searchRequest.Tenant}");
                }

                _logger.Information($"searching for groups with SearchText={searchRequest.SearchText}, SearchType={searchRequest.Type} {tenantInfo}");

                var groups = await _searchService.SearchGroupsAsync(searchRequest.SearchText, searchRequest.Type, SearchTypes.Exact);

                principals.AddRange(groups.Select(g => new FabricGroupApiModel
                {
                    FirstName = g.GroupFirstName,
                    GroupId = g.GroupId,
                    TenantId = g.TenantId,
                    PrincipalType = g.PrincipalType
                }));

                return new IdpSearchResultApiModel<FabricGroupApiModel>
                {
                    Principals = principals,
                    ResultCount = principals.Count
                };
            }
            catch (InvalidExternalIdentityProviderException e)
            {
                return CreateFailureResponse<FabricGroupApiModel>(e.Message, HttpStatusCode.BadRequest);
            }
            catch (BadRequestException e)
            {
                return CreateFailureResponse<FabricGroupApiModel>(e.Message, HttpStatusCode.BadRequest);
            }
        }

        private async Task<dynamic> SearchAsync()
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

                _logger.Information($"searching for users with SearchText={searchRequest.SearchText}, SearchType={searchRequest.Type}");

                var users = await _searchService.SearchPrincipalsAsync(searchRequest.SearchText, searchRequest.Type, SearchTypes.Wildcard);

                principals.AddRange(users.Select(u => new FabricPrincipalApiModel
                {
                    UserPrincipal = u.UserPrincipal,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    SubjectId = u.SubjectId,
                    PrincipalType = u.PrincipalType.ToString().ToLower()
                }));

                return new IdpSearchResultApiModel<FabricPrincipalApiModel>
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

        private async Task<dynamic> SearchByIdpAsync()
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

                _logger.Information($"searching for users with SearchText={searchRequest.SearchText}, SearchType={searchRequest.Type}");

                var users = await _searchService.SearchPrincipalsAsync(searchRequest.SearchText, searchRequest.Type, SearchTypes.Wildcard);

                principals.AddRange(users.Select(u => new FabricPrincipalApiModel
                {
                    UserPrincipal = u.UserPrincipal,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    SubjectId = u.SubjectId,
                    PrincipalType = u.PrincipalType.ToString().ToLower()
                }));

                return new IdpSearchResultApiModel<FabricPrincipalApiModel>
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
