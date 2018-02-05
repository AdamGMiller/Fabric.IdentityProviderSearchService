using System.Collections.Generic;
using Fabric.ActiveDirectory.Models;

namespace Fabric.ActiveDirectory.Services
{
    public class ActiveDirectoryProviderService : IExternalIdentityProviderService
    {

        public ICollection<ExternalUser> SearchUsers(string searchText)
        {
            var ldapQuery = $"(&(objectClass=user)(objectCategory=person)(|(sAMAccountName={searchText}*)(givenName={searchText}*)(sn={searchText}*)))";

            var users = new List<ExternalUser>();
            var user = new ExternalUser
            {
                FirstName = "sey",
                LastName = "butts",
                MiddleName = "mour",
                SubjectId = "testresult"
            };

            users.Add(user);
            return users;
        }
    }
}