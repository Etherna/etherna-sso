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

using Etherna.SSOServer.Domain.Models;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etherna.SSOServer.Configs.Hangfire
{
    internal sealed class AdminAuthFilter : IDashboardAuthorizationFilter
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
