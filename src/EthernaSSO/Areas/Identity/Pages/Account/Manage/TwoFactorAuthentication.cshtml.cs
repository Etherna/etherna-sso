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
using Etherna.SSOServer.Domain.Models.Fido2CredentialAgg;
using Etherna.SSOServer.Models;
using Etherna.SSOServer.Pages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class TwoFactorAuthenticationModel(
        UserManager<UserBase> userManager,
        SignInManager<UserBase> signInManager)
        : StatusMessagePageModel
    {
        // Properties.
        public IReadOnlyList<Fido2Credential> Fido2Credentials { get; set; } = [];
        public bool HasAuthenticator { get; set; }
        public bool HasFido2Credentials { get; set; }
        public bool Is2faEnabled { get; set; }
        public bool IsMachineRemembered { get; set; }
        public int RecoveryCodesLeft { get; set; }

        // Methods.
        public async Task<IActionResult> OnGet()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            Is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);

            if (user is UserWeb2 userWeb2)
            {
                Fido2Credentials = userWeb2.Fido2Credentials.ToList();
                HasAuthenticator = userWeb2.IsAuthenticatorAppEnabled;
                HasFido2Credentials = userWeb2.HasFido2Credentials;
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            await signInManager.ForgetTwoFactorClientAsync();
            StatusMessage = new StatusMessage("The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.");
            return RedirectToPage();
        }
    }
}
