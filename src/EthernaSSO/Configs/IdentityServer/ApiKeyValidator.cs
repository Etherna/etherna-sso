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
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            // Get user data.
            var user = await userManager.FindByNameAsync(context.UserName);
            if (user is null)
            {
                logger.NoUserFoundMatchingUsername(context.UserName);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "No user found matching username: {username}");
                return;
            }

            // Check if user is locked out.
            if (await userManager.IsLockedOutAsync(user))
            {
                logger.LockedOutLoginAttempt(user.Id);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User is locked out.");
                return;
            }

            // Check if user is allowed to sign in.
            if (!await signInManager.CanSignInAsync(user))
            {
                logger.NotAllowedSingInLoginAttempt(user.Id);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User is not allowed to sign in.");
                return;
            }

            // Get key data.
            var apiKey = await ssoDbContext.ApiKeys.TryFindOneAsync(k => k.KeyHash == ApiKey.HashKey(context.Password) &&
                                                                         k.Owner.Id == user.Id);
            if (apiKey is null)
            {
                //increment access failed counter
                await userManager.AccessFailedAsync(user);

                logger.ApiKeyDoesNotExistLoginAttempt(user.Id);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Api key does not exist.");
                return;
            }

            // Check if key is alive.
            if (!apiKey.IsAlive)
            {
                logger.ApiKeyIsNotAliveLoginAttempt(user.Id);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Api key is not alive.");
                return;
            }

            // Check key permissions on required scopes.
            //TODO

            logger.ApiKeyValidatedLoginAttempt(user.Id);

            context.Result = new GrantValidationResult(
                user.Id,
                OidcConstants.AuthenticationMethods.Password);
        }
    }
}
