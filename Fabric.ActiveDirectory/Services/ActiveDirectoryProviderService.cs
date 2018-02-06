﻿using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using Fabric.ActiveDirectory.Models;

namespace Fabric.ActiveDirectory.Services
{
    public class ActiveDirectoryProviderService : IExternalIdentityProviderService
    {
        private readonly string _domain;

        public ActiveDirectoryProviderService(string domain)
        {
            _domain = domain;
        }

        public ICollection<AdPrincipal> SearchUsers(string searchText)
        {
            var principals = new List<AdPrincipal>();

            var ctx = new PrincipalContext(ContextType.Domain, _domain);

            var userPrincipal = new UserPrincipal(ctx);

            var queryFilter = (Principal) userPrincipal;
            queryFilter.Name = $"{searchText}*";

            var searcher = new PrincipalSearcher(queryFilter);
            var principalresult =  searcher.FindAll();           

            foreach (var principal in principalresult)
            {
                var adPrincipal = new AdPrincipal {Name = principal.Name};

                if (principal is UserPrincipal)
                {
                    var userPrincipalResult = principal as UserPrincipal;
                    
                    adPrincipal.FirstName = userPrincipalResult.GivenName;
                    adPrincipal.MiddleName = userPrincipalResult.MiddleName;
                    adPrincipal.LastName = userPrincipalResult.Surname;
                    adPrincipal.PrincipalType = PrincipalType.User;
                }
                else if (principal is GroupPrincipal)
                {
                    adPrincipal.PrincipalType = PrincipalType.Group;
                }

                principals.Add(adPrincipal);
            }

            return principals;
        }
    }
}