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
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext context;
        private readonly IUserService userService;

        // Constructor.
        public UserDeleteModel(
            ISsoDbContext context,
            IUserService userService)
        {
            this.context = context;
            this.userService = userService;
        }

        // Properties.
        public string Id { get; private set; } = default!;
        public string Username { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            Id = id;
            var user = await context.Users.FindOneAsync(id);
            Username = user.Username;
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await context.Users.FindOneAsync(id);
            await userService.DeleteAsync(user);
            return RedirectToPage("Users");
        }
    }
}
