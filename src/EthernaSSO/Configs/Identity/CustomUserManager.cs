// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.Authentication;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Etherna.SSOServer.Configs.Identity
{
    public class CustomUserManager : UserManager<UserBase>
    {
        // Fields.
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;

        // Constructor.
        public CustomUserManager(
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IUserStore<UserBase> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<UserBase> passwordHasher,
            IEnumerable<IUserValidator<UserBase>> userValidators,
            IEnumerable<IPasswordValidator<UserBase>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<UserBase>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.ethernaOidcClient = ethernaOidcClient;
        }

        // Methods.
        public override string? GetUserId(ClaimsPrincipal principal)
        {
            var id = base.GetUserId(principal);

            // If is null, try to get from Oidc.
            if (id is null)
            {
                var getIdTask = ethernaOidcClient.TryGetUserIdAsync();
                getIdTask.Wait();
                id = getIdTask.Result;
            }

            return id;
        }

        public override string? GetUserName(ClaimsPrincipal principal)
        {
            var username = base.GetUserName(principal);

            // If is null, try to get from Oidc.
            if (username is null)
            {
                var getUsernameTask = ethernaOidcClient.TryGetUsernameAsync();
                getUsernameTask.Wait();
                username = getUsernameTask.Result;
            }

            return username;
        }
    }
}
