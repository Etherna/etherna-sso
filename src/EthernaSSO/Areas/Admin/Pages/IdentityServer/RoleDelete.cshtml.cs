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
