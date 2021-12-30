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
            if (id is null)
                throw new ArgumentNullException(nameof(id));

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
