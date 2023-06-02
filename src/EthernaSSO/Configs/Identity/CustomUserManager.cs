//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
