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
using Etherna.SSOServer.Models;
using Etherna.SSOServer.Pages;
using Etherna.SSOServer.Services.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class DisableAuthenticatorAppModel(
        ILogger<DisableAuthenticatorAppModel> logger,
        SignInManager<UserBase> signInManager,
        UserManager<UserBase> userManager)
        : StatusMessagePageModel
    {
        // Properties.
        public bool IsLastFactor { get; set; }

        // Methods.
        public async Task<IActionResult> OnGet()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not UserWeb2 userWeb2)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!userWeb2.IsAuthenticatorAppEnabled)
                throw new InvalidOperationException($"Cannot disable authenticator app for user with ID '{userManager.GetUserId(User)}' as it's not currently configured.");

            IsLastFactor = !userWeb2.HasFido2Credentials;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not UserWeb2 userWeb2)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!userWeb2.IsAuthenticatorAppEnabled)
                throw new InvalidOperationException($"Cannot disable authenticator app for user with ID '{userManager.GetUserId(User)}' as it's not currently configured.");

            userWeb2.DisableAuthenticatorApp();

            if (!userWeb2.HasFido2Credentials)
            {
                if (await signInManager.IsTwoFactorClientRememberedAsync(user))
                    await signInManager.ForgetTwoFactorClientAsync();
                await userManager.SetTwoFactorEnabledAsync(user, false);
            }

            await userManager.UpdateAsync(user);

            logger.Disabled2FA(userManager.GetUserId(User) ?? throw new InvalidOperationException());
            StatusMessage = new StatusMessage(userWeb2.HasFido2Credentials
                ? "Authenticator app disabled. Your security keys are still active."
                : "Two-factor authentication has been disabled. You can re-enable it any time.");
            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}
