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

            logger.Disabled2FA(userManager.GetUserId(User) ?? throw new InvalidOperationException());
            StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}
