﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fabric.IdentityProviderSearchService.Models;
using Fabric.IdentityProviderSearchService.Services.Azure;
using Microsoft.Graph;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class AzureDirectoryProviderService : IExternalIdentityProviderService
    {
        private IGraphServiceClient _client;
        private IAzureActiveDirectoryClientCredentialsService _azureService;

        public AzureDirectoryProviderService(IGraphServiceClient client, IAzureActiveDirectoryClientCredentialsService azureADClientCredentialsService)
        {
            _client = client;
            _azureService = azureADClientCredentialsService;
        }

        public IFabricPrincipal FindUserBySubjectId(string subjectId)
        {
            var result = GetUserAsync(subjectId).Result;
            var principal = CreateUserPrincipal(result);

            return principal;
        }

        public IEnumerable<IFabricPrincipal> SearchPrincipals(string searchText, PrincipalType principalType)
        {
            switch(principalType)
            {
                case PrincipalType.User:
                    return GetUserPrincipalsAsync(searchText).Result;
                case PrincipalType.Group:
                    return GetGroupPrincipalsAsync(searchText).Result;
                default:
                    return GetUserAndGroupPrincipalsAsync(searchText).Result;
            }
        }

        private async Task<User> GetUserAsync(string subjectId)
        {
            return await _client.Users[subjectId].Request().GetAsync();
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetUserAndGroupPrincipalsAsync(string searchText)
        {
            var results = await Task.WhenAll(
                Task.Run(() => GetUserPrincipalsAsync(searchText)),
                Task.Run(() => GetGroupPrincipalsAsync(searchText))
            );
            return results.SelectMany(result => result);
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetUserPrincipalsAsync(string searchText)
        {
            var filterQuery = 
                $"startswith(DisplayName, '{searchText}') or startswith(GivenName, '{searchText}') or startswith(PreferredName, '{searchText}') or startswith(UserPrincipalName, '{searchText}')";
            var principals = new List<IFabricPrincipal>();
            var users = await _client.Users.Request().Filter(filterQuery).GetAsync();
            foreach(var result in users)
            {
                principals.Add(CreateUserPrincipal(result));
            }

            return principals;
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetGroupPrincipalsAsync(string searchText)
        {
            var filterQuery = $"startswith(DisplayName, '{searchText}')";
            var principals = new List<IFabricPrincipal>();
            var groups = await _client.Groups.Request().Filter(filterQuery).GetAsync();

            foreach(var result in groups)
            {
                principals.Add(CreateGroupPrincipal(result));
            }

            return principals;
        }

        private IFabricPrincipal CreateUserPrincipal(User userEntry)
        {
            return new FabricPrincipal
            {
                FirstName = userEntry.GivenName,
                LastName = userEntry.Surname,
                MiddleName = string.Empty,   // don't think this has a value in graph api/azure ad
                PrincipalType = PrincipalType.User,
                SubjectId = userEntry.Id
            };
        }

        private IFabricPrincipal CreateGroupPrincipal(Group groupEntry)
        {
            return new FabricPrincipal
            {
                SubjectId = groupEntry.Id,
                PrincipalType = PrincipalType.Group
            };
        }
    }
}