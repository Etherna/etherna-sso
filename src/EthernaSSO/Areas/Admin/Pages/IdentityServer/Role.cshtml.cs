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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class RoleModel : PageModel
    {
        // Model.
        public class InputModel
        {
            [Required]
            [Display(Name = "Role name")]
            public string Name { get; set; } = default!;
        }

        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public RoleModel(ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string? Id { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task OnGetAsync(string? id)
        {
            Id = id;
            Input = new InputModel();

            if (id is not null)
            {
                var role = await context.Roles.FindOneAsync(id);
                Input.Name = role.Name;
            }
        }

        public async Task<IActionResult> OnPostSaveAsync(string? id)
        {
            Id = id;

            if (!ModelState.IsValid)
                return Page();

            Role role;
            if (id is null) //create
            {
                role = new Role(Input.Name);
                await context.Roles.CreateAsync(role);
            }
            else //update
            {
                role = await context.Roles.FindOneAsync(id);
                role.SetName(Input.Name);
                await context.SaveChangesAsync();
            }

            return RedirectToPage(new { role.Id });
        }
    }
}
