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
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etherna.SSOServer.Configs.Hangfire
{
    public class AdminAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            if (httpContext?.User is null)
                return false;
            var userManager = httpContext.RequestServices.GetService<UserManager<UserBase>>()!;

            var getUserTask = userManager.GetUserAsync(httpContext.User);
            getUserTask.Wait();
            var user = getUserTask.Result ?? throw new InvalidOperationException();

            var isInRoleTask = userManager.IsInRoleAsync(user, Role.AdministratorName);
            isInRoleTask.Wait();
            return isInRoleTask.Result;
        }
    }
}
