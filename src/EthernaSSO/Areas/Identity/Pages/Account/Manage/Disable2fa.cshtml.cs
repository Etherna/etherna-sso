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
using Etherna.SSOServer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class Disable2faModel : PageModel
    {
        // Fields.
        private readonly ILogger<Disable2faModel> logger;
        private readonly SignInManager<UserBase> signInManager;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public Disable2faModel(
            ILogger<Disable2faModel> logger,
            SignInManager<UserBase> signInManager,
            UserManager<UserBase> userManager)
        {
            this.logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        // Properties.
        [TempData]
        public string? StatusMessage { get; set; }

        // Methods.
        public async Task<IActionResult> OnGet()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!await userManager.GetTwoFactorEnabledAsync(user))
                throw new InvalidOperationException($"Cannot disable 2FA for user with ID '{userManager.GetUserId(User)}' as it's not currently enabled.");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (await signInManager.IsTwoFactorClientRememberedAsync(user))
                await signInManager.ForgetTwoFactorClientAsync();

            var disable2faResult = await userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{userManager.GetUserId(User)}'.");

            logger.Disabled2FA(userManager.GetUserId(User));
            StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}
