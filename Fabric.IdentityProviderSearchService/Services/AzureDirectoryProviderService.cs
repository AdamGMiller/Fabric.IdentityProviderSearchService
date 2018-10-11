using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fabric.IdentityProviderSearchService.Models;
using Microsoft.Graph;

namespace Fabric.IdentityProviderSearchService.Services
{
    public class AzureDirectoryProviderService : IExternalIdentityProviderService
    {
        private IGraphServiceClient _client;

        public AzureDirectoryProviderService(IGraphServiceClient client)
        {
            _client = client;
        }

        public IFabricPrincipal FindUserBySubjectId(string subjectId)
        {
            var result = GetUserAsync(subjectId).Result;
            var principal = CreateUserPrincipal(result.First());

            return principal;
        }

        public IEnumerable<IFabricPrincipal> SearchPrincipals(string searchText, PrincipalType principalType)
        {
            switch(principalType)
            {
                case PrincipalType.User:
                    return GetUserPrincipalsAsync().Result;
                case PrincipalType.Group:
                    return GetGroupPrincipalsAsync().Result;
                default:
                    return GetUsersAndGroups();
            }

            throw new NotImplementedException();
        }

        private async Task<IGraphServiceUsersCollectionPage> GetUserAsync(string subjectId)
        {
            // curently gets all users
            return await _client.Users.Request().GetAsync();
        }

        private IEnumerable<IFabricPrincipal> GetUsersAndGroups()
        {
            var userPrincipals = GetUserPrincipalsAsync();
            var groupPrincipals = GetGroupPrincipalsAsync();

            var principals = userPrincipals.Result;
            principals.Concat(groupPrincipals.Result);

            return principals;
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetUserPrincipalsAsync()
        {
            var principals = new List<IFabricPrincipal>();
            var users = await _client.Users.Request().GetAsync();
            foreach(var result in users)
            {
                principals.Add(CreateUserPrincipal(result));
            }

            return principals;
        }

        private async Task<IEnumerable<IFabricPrincipal>> GetGroupPrincipalsAsync()
        {
            // can we avoid awaiting here?
            var principals = new List<IFabricPrincipal>();
            var groups = await _client.Groups.Request().GetAsync();

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
                FirstName = userEntry.GivenName,    // Given name is first name
                LastName = userEntry.GivenName, // how to get these values
                MiddleName = userEntry.GivenName,   // how to get these values
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