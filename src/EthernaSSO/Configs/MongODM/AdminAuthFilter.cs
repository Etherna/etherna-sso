// Copyright 2021-present Etherna Sa
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
