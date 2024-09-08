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
                ArgumentNullException.ThrowIfNull(role, nameof(role));

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
            ArgumentNullException.ThrowIfNull(id, nameof(id));

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
