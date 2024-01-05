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

using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UsersModel : PageModel
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
        private readonly IUserService userService;

        // Constructor.
        public UsersModel(
            IUserService userService)
        {
            this.userService = userService;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
        public string? Query { get; private set; }
        public List<UserDto> Users { get; } = new();

        // Methods.
        public async Task OnGetAsync(int? p, string? q)
        {
            CurrentPage = p ?? 0;
            Query = q ?? "";

            var paginatedUsers = await userService.SearchPaginatedUsersByQueryAsync(
                Query, u => u.Username, CurrentPage, PageSize);

            MaxPage = paginatedUsers.MaxPage;

            Users.AddRange(paginatedUsers.Elements.Select(u => new UserDto(u)));
        }
    }
}
