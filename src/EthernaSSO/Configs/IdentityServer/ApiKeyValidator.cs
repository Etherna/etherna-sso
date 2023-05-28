using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.IdentityServer
{
    internal sealed class ApiKeyValidator : IResourceOwnerPasswordValidator
    {
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        public ApiKeyValidator(
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            // Get data.
            var user = await userManager.FindByNameAsync(context.UserName);
            if (user is null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User does not exist.");
                return;
            }

            var apiKey = await ssoDbContext.ApiKeys.TryFindOneAsync(k => k.KeyHash == ApiKey.HashKey(context.Password) &&
                                                                         k.Owner.Id == user.Id);
            if (apiKey is null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Api key does not exist.");
                return;
            }

            // Check if key is alive.
            if (apiKey.IsAlive)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Api key is not alive.");
                return;
            }

            // Check key permissions on required scopes.
            //TODO

            context.Result = new GrantValidationResult(
                subject: user.Id,
                authenticationMethod: OidcConstants.AuthenticationMethods.Password);
        }
    }
}
