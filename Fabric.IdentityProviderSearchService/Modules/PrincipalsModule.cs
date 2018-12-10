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

            Get("{identityProvider}/search",
                async _ => await SearchByIdpAsync().ConfigureAwait(false),
                null,
                "SearchByIdpAsync");

            Get("/user",
                async _ => await SearchForUserAsync().ConfigureAwait(false),
                null,
                "SearchForUserAsync");

            Get("/{identityProvider}/groups/{groupName}",
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
                var principals = new List<FabricPrincipalApiModel>();
                string tenantInfo = null;

                if (!string.IsNullOrEmpty(searchRequest.TenantId))
                {
                    tenantInfo = ($", TenantId={searchRequest.TenantId}");
                }

                _logger.Information($"searching for user with subject id: {searchRequest.SubjectId} {tenantInfo}");
                var user = await _searchService.FindUserBySubjectIdAsync(searchRequest.SubjectId, searchRequest.TenantId);

                return new FabricPrincipalApiModel
                    {
                        FirstName = user?.FirstName,
                        LastName = user?.LastName,
                        MiddleName = user?.MiddleName,
                        SubjectId = user?.SubjectId,
                        TenantId = user?.TenantId,
                        PrincipalType = user?.PrincipalType.ToString().ToLower(),
                        IdentityProviderUserPrincipalName = user?.IdentityProviderUserPrincipalName
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
            var searchRequest = this.Bind<SearchGroupRequest>();
            searchRequest.Type = PrincipalType.Group.ToString();

            if (string.IsNullOrEmpty(searchRequest.GroupName))
            {
                return CreateFailureResponse<FabricGroupApiModel>("GroupName was not provided and is required",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                var principals = new List<FabricGroupApiModel>();
                string tenantId = null;

                if (Request.Query["tenantId"].HasValue)
                {
                    tenantId = Request.Query["tenantId"].ToString();
                }

                _logger.Information($"searching for groups with IdentityProvider={searchRequest.IdentityProvider}, GroupName={searchRequest.GroupName}, SearchType={searchRequest.Type} {tenantId}");

                var groups = await _searchService.SearchPrincipalsAsync<IFabricGroup>(searchRequest.GroupName, searchRequest.Type, SearchTypes.Exact, tenantId);

                principals.AddRange(groups.Select(g => new FabricGroupApiModel
                {
                    ExternalIdentifier = g.ExternalIdentifier,
                    GroupName = g.GroupName,
                    TenantId = g.TenantId,
                    IdentityProvider = searchRequest.IdentityProvider,
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

                _logger.Information($"searching for groups with IdentityProvider={searchRequest.IdentityProvider}, GroupName={searchRequest.SearchText}, SearchType={searchRequest.Type} {searchRequest.TenantId}");

                var usersAndGroups = await _searchService.SearchPrincipalsAsync<IFabricPrincipal>(searchRequest.SearchText, searchRequest.Type, SearchTypes.Wildcard, searchRequest.TenantId).ConfigureAwait(false);

                principals.AddRange(usersAndGroups.Select(ug => new FabricPrincipalApiModel
                {
                    UserPrincipal = ug.UserPrincipal,
                    FirstName = ug.FirstName,
                    MiddleName = ug.MiddleName,
                    LastName = ug.LastName,
                    DisplayName = ug.DisplayName,
                    SubjectId = ug.SubjectId,
                    ExternalIdentifier = ug.ExternalIdentifier,
                    TenantId = ug.TenantId,
                    IdentityProvider = ug.IdentityProvider,
                    PrincipalType = ug.PrincipalType.ToString().ToLower(),
                    IdentityProviderUserPrincipalName = ug.IdentityProviderUserPrincipalName
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

                string tenantInfo = null;

                if (!string.IsNullOrEmpty(searchRequest.TenantId))
                {
                    tenantInfo = ($", TenantId={searchRequest.TenantId}");
                }

                _logger.Information($"searching for groups with IdentityProvider={searchRequest.IdentityProvider}, GroupName={searchRequest.SearchText}, SearchType={searchRequest.Type} {tenantInfo}");

                var usersAndGroups = await _searchService.SearchPrincipalsAsync<IFabricPrincipal>(searchRequest.SearchText, searchRequest.Type, SearchTypes.Wildcard, searchRequest.TenantId);

                principals.AddRange(usersAndGroups.Select(ug => new FabricPrincipalApiModel
                {
                    UserPrincipal = ug.UserPrincipal,
                    FirstName = ug.FirstName,
                    MiddleName = ug.MiddleName,
                    LastName = ug.LastName,
                    DisplayName = ug.DisplayName,
                    SubjectId = ug.SubjectId,
                    ExternalIdentifier = ug.ExternalIdentifier,
                    TenantId = ug.TenantId,
                    IdentityProvider = searchRequest.IdentityProvider,
                    PrincipalType = ug.PrincipalType.ToString().ToLower(),
                    IdentityProviderUserPrincipalName = ug.IdentityProviderUserPrincipalName
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
