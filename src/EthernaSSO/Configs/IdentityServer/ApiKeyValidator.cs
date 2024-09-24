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

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.IdentityServer
{
    internal sealed class ApiKeyValidator : IResourceOwnerPasswordValidator
    {
        private readonly ILogger<ApiKeyValidator> logger;
        private readonly SignInManager<UserBase> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        public ApiKeyValidator(
            ILogger<ApiKeyValidator> logger,
            SignInManager<UserBase> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            this.logger = logger;
            this.signInManager = signInManager;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            /*
             * The ROPC base protocol requires to pass fields "username" and "password" during authentication.
             * Instead, in our case we decided to pass user's Id in place of username, and api key in place of password.
             */
            var userId = context.UserName;
            var plainApiKey = context.Password;

            // Get user data.
            var user = await ssoDbContext.Users.TryFindOneAsync(userId);
            if (user is null)
            {
                logger.NoUserFoundWithId(userId);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, $"No user found with Id {userId}");
                return;
            }

            // Check if user is locked out.
            if (await userManager.IsLockedOutAsync(user))
            {
                logger.LockedOutLoginAttempt(userId);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User is locked out.");
                return;
            }

            // Check if user is allowed to sign in.
            if (!await signInManager.CanSignInAsync(user))
            {
                logger.NotAllowedSingInLoginAttempt(userId);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User is not allowed to sign in.");
                return;
            }

            // Get key data.
            var apiKey = await ssoDbContext.ApiKeys.TryFindOneAsync(k => k.KeyHash == ApiKey.HashKey(plainApiKey) &&
                                                                         k.Owner.Id == userId);
            if (apiKey is null)
            {
                //increment access failed counter
                await userManager.AccessFailedAsync(user);

                logger.ApiKeyDoesNotExistLoginAttempt(userId);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Api key does not exist.");
                return;
            }

            // Check if key is alive.
            if (!apiKey.IsAlive)
            {
                logger.ApiKeyIsNotAliveLoginAttempt(userId);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Api key is not alive.");
                return;
            }

            // Check key permissions on required scopes.
            //TODO

            logger.ApiKeyValidatedLoginAttempt(userId);

            context.Result = new GrantValidationResult(
                userId,
                OidcConstants.AuthenticationMethods.Password);
        }
    }
}
