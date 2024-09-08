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

using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class RoleDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public RoleDeleteModel(ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string Id { get; private set; } = default!;

        [Display(Name = "Role name")]
        public string Name { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            Id = id;
            var role = await context.Roles.FindOneAsync(id);
            Name = role.Name;
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            var role = await context.Roles.FindOneAsync(id);
            var usersWithRole = await context.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Roles.Contains(role))
                        .ToListAsync());

            foreach (var user in usersWithRole)
                user.RemoveRole(role.Name);
            await context.SaveChangesAsync();

            await context.Roles.DeleteAsync(id);

            return RedirectToPage("Roles");
        }
    }
}
