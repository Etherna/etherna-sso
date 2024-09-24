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

using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserRolesDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserRolesDeleteModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string RoleId { get; private set; } = default!;
        public string RoleName { get; private set; } = default!;
        public string UserId { get; private set; } = default!;
        public string Username { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string roleId, string userId)
        {
            ArgumentNullException.ThrowIfNull(roleId, nameof(roleId));
            ArgumentNullException.ThrowIfNull(userId, nameof(userId));

            RoleId = roleId;
            var role = await context.Roles.FindOneAsync(roleId);
            RoleName = role.Name;

            UserId = userId;
            var user = await context.Users.FindOneAsync(userId);
            Username = user.Username;
        }

        public async Task<IActionResult> OnPostDeleteAsync(string roleId, string userId)
        {
            ArgumentNullException.ThrowIfNull(roleId, nameof(roleId));
            ArgumentNullException.ThrowIfNull(userId, nameof(userId));

            var role = await context.Roles.FindOneAsync(roleId);
            var user = await context.Users.FindOneAsync(userId);
            user.RemoveRole(role.Name);
            await context.SaveChangesAsync();

            return RedirectToPage("UserRoles", new { id = userId });
        }
    }
}
