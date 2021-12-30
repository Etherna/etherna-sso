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

using Etherna.MongoDB.Driver;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserRolesModel : PageModel
    {
        // Models.
        public class RoleDto
        {
            public RoleDto(Role role)
            {
                if (role is null)
                    throw new ArgumentNullException(nameof(role));

                Id = role.Id;
                Name = role.Name;
            }

            public string Id { get; }
            public string Name { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserRolesModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public IEnumerable<RoleDto> AllRoles { get; private set; } = default!;
        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public string RoleId { get; private set; } = default!;
        public string UserId { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public IEnumerable<RoleDto> UserRoles { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id, int? p)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var roles = await context.Roles.QueryElementsAsync(elements =>
                elements.ToListAsync());
            var user = await context.Users.FindOneAsync(id);

            AllRoles = roles.Select(r => new RoleDto(r));
            CurrentPage = p ?? 0;
            MaxPage = (user.Roles.Count() - 1) / PageSize;
            UserId = id;
            Username = user.Username;
            UserRoles = user.Roles.OrderBy(r => r.Name)
                                  .Skip(PageSize * CurrentPage)
                                  .Take(PageSize)
                                  .Select(r => new RoleDto(r));
        }

        public async Task<IActionResult> OnPostAsync(string userId, string? roleId)
        {
            if (roleId is null)
                return RedirectToPage(new { id = userId });

            var user = await context.Users.FindOneAsync(userId);
            var role = await context.Roles.FindOneAsync(roleId);

            user.AddRole(role);

            await context.SaveChangesAsync();

            return RedirectToPage(new { id = userId });
        }
    }
}
