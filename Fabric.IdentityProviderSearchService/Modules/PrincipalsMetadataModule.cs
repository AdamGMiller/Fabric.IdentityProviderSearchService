using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fabric.IdentityProviderSearchService.ApiModels;
using Fabric.IdentityProviderSearchService.Constants;
using Nancy;
using Nancy.Swagger;
using Nancy.Swagger.Modules;
using Nancy.Swagger.Services;
using Nancy.Swagger.Services.RouteUtils;
using Swagger.ObjectModel;
using Swagger.ObjectModel.Builders;

namespace Fabric.IdentityProviderSearchService.Modules
{
    public class PrincipalsMetadataModule : SwaggerMetadataModule
    {
        private readonly Parameter _searchTextParameter = new Parameter
        {
            Name = "searchtext",
            Description = "The string to use for searching users and groups",
            Required = true,
            Type = "string",
            In = ParameterIn.Query            
        };

        private readonly Parameter _typeParameter = new Parameter
        {
            Name = "type",
            Description = "Filters the search to either users or groups. Valid values are 'user' and 'group'. If not specified then both users and groups are searched",
            Required = false,
            Type = "string",
            In = ParameterIn.Query
        };

        private readonly Tag _searchTag =
            new Tag { Name = "Search", Description = "Finding users and groups from an identity provider" };

        private readonly SecurityRequirementBuilder _oAuth2SearchScopeBuilder = new SecurityRequirementBuilder()
            .SecurityScheme(SecuritySchemes.Oauth2)
            .SecurityScheme(new List<string> { Scopes.SearchPrincipalsScope });

        public PrincipalsMetadataModule(ISwaggerModelCatalog modelCatalog, ISwaggerTagCatalog tagCatalog)
            : base(modelCatalog, tagCatalog)
        {

            modelCatalog.AddModel<FabricPrincipalApiModel>();

            RouteDescriber.DescribeRouteWithParams(
                "Search",
                "",
                "Searches for users and groups in an identity provider",
                new[]
                {
                    new HttpResponseMetadata<IdpSearchResultApiModel>
                    {
                        Code = (int) HttpStatusCode.OK,
                        Message = "Search was successful"
                    },
                    new HttpResponseMetadata
                    {
                        Code = (int) HttpStatusCode.BadRequest,
                        Message = "Invalid type parameter provided"
                    }
                },
                new[]
                {
                    _searchTextParameter,
                    _typeParameter
                },
                new[]
                {
                    _searchTag
                }).SecurityRequirement(_oAuth2SearchScopeBuilder);


        }
    }
}