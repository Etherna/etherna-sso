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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserProvidersDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserProvidersDeleteModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string Id { get; private set; } = default!;

        [Display(Name = "Login provider")]
        public string LoginProvider { get; private set; } = default!;

        [Display(Name = "Provider display name")]
        public string? ProviderDisplayName { get; private set; } = default!;

        [Display(Name = "Provider key")]
        public string ProviderKey { get; private set; } = default!;

        public string Username { get; private set; } = default!;

        // Methods.
        public async Task<IActionResult> OnGet(string id, string loginProvider, string providerKey)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));
            if (loginProvider is null)
                throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey is null)
                throw new ArgumentNullException(nameof(providerKey));

            var user = await context.Users.FindOneAsync(id);
            if (user is not UserWeb2 userWeb2)
                return RedirectToPage("User", new { id });

            Initialize(loginProvider, providerKey, userWeb2);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id, string loginProvider, string providerKey)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var user = await context.Users.FindOneAsync(id);
            if (user is not UserWeb2 userWeb2)
                return RedirectToPage("User", new { id });

            try
            {
                userWeb2.RemoveExternalLogin(loginProvider, providerKey);
                await context.SaveChangesAsync();
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, "Can't remove external login");

                Initialize(loginProvider, providerKey, userWeb2);
                return Page();
            }

            return RedirectToPage("UserProviders", new { id });
        }

        // Helpers.
        private void Initialize(string loginProvider, string providerKey, UserWeb2 userWeb2)
        {
            var login = userWeb2.Logins.First(
                l => l.LoginProvider == loginProvider &&
                l.ProviderKey == providerKey);

            Id = userWeb2.Id;
            LoginProvider = login.LoginProvider;
            ProviderDisplayName = login.ProviderDisplayName;
            ProviderKey = login.ProviderKey;
            Username = userWeb2.Username;
        }
    }
}
