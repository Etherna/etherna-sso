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
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class RoleUsersModel : PageModel
    {
        // Models.
        public class UserDto
        {
            public UserDto(UserBase user)
            {
                ArgumentNullException.ThrowIfNull(user, nameof(user));

                Id = user.Id;
                Email = user.Email;
                EtherAddress = user.EtherAddress;
                Username = user.Username;
            }

            public string Id { get; }
            public string? Email { get; }
            public string EtherAddress { get; }
            public string Username { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly ISsoDbContext context;
        private readonly IUserService userService;

        // Constructor.
        public RoleUsersModel(
            ISsoDbContext context,
            IUserService userService)
        {
            this.context = context;
            this.userService = userService;
        }

        // Properties.
        public int CurrentPage { get; set; }
        public long MaxPage { get; set; }
        public string? Query { get; set; }
        public string RoleId { get; private set; } = default!;
        public string RoleName { get; private set; } = default!;
        public List<UserDto> Users { get; } = new();

        // Methods.
        public async Task OnGetAsync(string id, int? p, string? q)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            CurrentPage = p ?? 0;
            Query = q ?? "";
            RoleId = id;

            var role = await context.Roles.FindOneAsync(id);
            RoleName = role.Name;

            var paginatedUsers = await userService.SearchPaginatedUsersByQueryAsync(
                Query, u => u.Username, CurrentPage, PageSize, u => u.Roles.Contains(role));

            MaxPage = paginatedUsers.MaxPage;

            Users.AddRange(paginatedUsers.Elements.Select(u => new UserDto(u)));
        }
    }
}
