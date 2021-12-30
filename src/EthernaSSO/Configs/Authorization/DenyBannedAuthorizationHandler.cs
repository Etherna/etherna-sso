using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.Authorization
{
    public class DenyBannedAuthorizationHandler : AuthorizationHandler<DenyBannedAuthorizationRequirement>
    {
        // Fields.
        private readonly UserManager<UserBase> userManager;

        // Constructor
        public DenyBannedAuthorizationHandler(UserManager<UserBase> userManager)
        {
            this.userManager = userManager;
        }

        // Methods.
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DenyBannedAuthorizationRequirement requirement)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (await userManager.IsLockedOutAsync(user))
                    context.Fail();
                else
                    context.Succeed(requirement);
            }
        }
    }
}
