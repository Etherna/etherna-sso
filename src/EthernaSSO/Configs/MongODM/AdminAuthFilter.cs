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

using Etherna.MongODM.AspNetCore.UI.Auth.Filters;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.MongODM
{
    public class AdminAuthFilter : IDashboardAuthFilter
    {
        public async Task<bool> AuthorizeAsync(HttpContext? context)
        {
            if (context?.User is null)
                return false;
            var userManager = context.RequestServices.GetService<UserManager<UserBase>>()!;

            var user = await userManager.GetUserAsync(context.User) ?? throw new InvalidOperationException();

            return await userManager.IsInRoleAsync(user, Role.AdministratorName);
        }
    }
}
