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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class ExternalLoginsModel : PageModel
    {
        // Fields.
        private readonly SignInManager<UserBase> signInManager;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public ExternalLoginsModel(
            SignInManager<UserBase> signInManager,
            UserManager<UserBase> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        // Properties.
        public List<UserLoginInfo> CurrentLogins { get; } = new List<UserLoginInfo>();
        public List<AuthenticationScheme> OtherLogins { get; } = new List<AuthenticationScheme>();
        public bool ShowRemoveButton { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = default!;

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            if (await userManager.GetUserAsync(User) is not UserWeb2 user)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            CurrentLogins.AddRange(await userManager.GetLoginsAsync(user));
            OtherLogins.AddRange((await signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider)));
            ShowRemoveButton = user.CanRemoveExternalLogin;

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user.");

            var result = await userManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not removed.";
                return RedirectToPage();
            }

            await signInManager.RefreshSignInAsync(user);
            StatusMessage = "The external login was removed.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user.");

            var info = await signInManager.GetExternalLoginInfoAsync(user.Id);
            if (info == null)
                throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");

            var result = await userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not added. External logins can only be associated with one account.";
                return RedirectToPage();
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = "The external login was added.";
            return RedirectToPage();
        }
    }
}
