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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
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
            public UserDto(
                string id,
                string? email,
                string etherAddress,
                string username)
            {
                Id = id;
                Email = email;
                EtherAddress = etherAddress;
                Username = username;
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
        public int MaxPage { get; set; }
        public string? Query { get; set; }
        public string RoleId { get; private set; } = default!;
        public string RoleName { get; private set; } = default!;
        public List<UserDto> Users { get; } = new();

        // Methods.
        public async Task OnGetAsync(string id, int? p, string? q)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            CurrentPage = p ?? 0;
            Query = q ?? "";
            RoleId = id;

            var role = await context.Roles.FindOneAsync(id);
            RoleName = role.Name;

            var paginatedUsers = await userService.SearchPaginatedUsersByQueryAsync(
                Query, u => u.Username, CurrentPage, PageSize, u => u.Roles.Contains(role));

            MaxPage = paginatedUsers.MaxPage;

            Users.AddRange(paginatedUsers.Elements.Select(u => new UserDto(u.Id, u.Email, u.EtherAddress, u.Username)));
        }
    }
}
