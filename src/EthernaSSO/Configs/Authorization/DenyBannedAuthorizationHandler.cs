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
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User) ?? throw new InvalidOperationException();

                if (await userManager.IsLockedOutAsync(user))
                    context.Fail();
                else
                    context.Succeed(requirement);
            }
        }
    }
}
