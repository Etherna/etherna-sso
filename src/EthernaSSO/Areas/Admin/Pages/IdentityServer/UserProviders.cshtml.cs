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
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserProvidersModel : PageModel
    {
        // Models.
        public class ProviderDto
        {
            public ProviderDto(UserLoginInfo externalLogin)
            {
                if (externalLogin is null)
                    throw new ArgumentNullException(nameof(externalLogin));

                LoginProvider = externalLogin.LoginProvider;
                ProviderDisplayName = externalLogin.ProviderDisplayName;
                ProviderKey = externalLogin.ProviderKey;
            }

            public string LoginProvider { get; }
            public string? ProviderDisplayName { get; }
            public string ProviderKey { get; }
        }

        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserProvidersModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string Id { get; private set; } = default!;
        public IEnumerable<ProviderDto> Providers { get; private set; } = Array.Empty<ProviderDto>();
        public string Username { get; private set; } = default!;

        // Methods.
        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            Id = id;
            var user = await context.Users.FindOneAsync(id);

            if (user is not UserWeb2 userWeb2)
                return RedirectToPage("User", new { id });

            Providers = userWeb2.Logins.Select(l => new ProviderDto(l));
            Username = userWeb2.Username;

            return Page();
        }
    }
}
