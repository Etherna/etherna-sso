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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.Fido2CredentialAgg;
using Etherna.SSOServer.Models;
using Etherna.SSOServer.Pages;
using Etherna.SSOServer.Services.Domain;
using Etherna.SSOServer.Services.Extensions;
using Fido2NetLib;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class AddSecurityKeyModel(
        IFido2Service fido2Service,
        ILogger<AddSecurityKeyModel> logger,
        ISsoDbContext ssoDbContext,
        UserManager<UserBase> userManager)
        : StatusMessagePageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [StringLength(Fido2Credential.MaxNicknameLength, MinimumLength = 1)]
            [Display(Name = "Name")]
            public string Nickname { get; set; } = null!;

            [Required]
            public string AttestationResponseJson { get; set; } = null!;
        }

        // Properties.
        public string CredentialOptionsJson { get; set; } = null!;

        [BindProperty]
        public InputModel Input { get; set; } = null!;

#pragma warning disable CA1819 // Properties should not return arrays
        [TempData]
        public string[] RecoveryCodes { get; set; } = null!; //array required by [TempData]
#pragma warning restore CA1819 // Properties should not return arrays

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not UserWeb2 userWeb2)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            var options = await fido2Service.BeginRegistrationAsync(userWeb2);
            await ssoDbContext.SaveChangesAsync();
            CredentialOptionsJson = options.ToJson();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not UserWeb2 userWeb2)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
                return await ReloadOptionsAsync(userWeb2);

            AuthenticatorAttestationRawResponse? attestation;
            try
            {
                attestation = System.Text.Json.JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(Input.AttestationResponseJson);
            }
            catch (System.Text.Json.JsonException)
            {
                ModelState.AddModelError(string.Empty, "Invalid attestation response.");
                return await ReloadOptionsAsync(userWeb2);
            }
            if (attestation is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid attestation response.");
                return await ReloadOptionsAsync(userWeb2);
            }

            try
            {
                await fido2Service.CompleteRegistrationAsync(userWeb2, attestation, Input.Nickname);
                await ssoDbContext.SaveChangesAsync();
            }
            catch (Fido2VerificationException ex)
            {
                ModelState.AddModelError(string.Empty, $"Security key registration failed: {ex.Message}");
                return await ReloadOptionsAsync(userWeb2);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return await ReloadOptionsAsync(userWeb2);
            }

            logger.AddedSecurityKey(userWeb2.Id, Input.Nickname);
            StatusMessage = new StatusMessage($"Security key '{Input.Nickname}' added.");

            //ensure a 2FA-enabled account always has a recovery path: if this is the first factor and the
            //user has no recovery codes yet, generate and show them once (mirrors AddAuthenticatorApp).
            if (await userManager.CountRecoveryCodesAsync(userWeb2) == 0)
            {
                var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(userWeb2, 10) ?? throw new InvalidOperationException();
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }

            return RedirectToPage("./TwoFactorAuthentication");
        }

        // Helpers.
        private async Task<IActionResult> ReloadOptionsAsync(UserWeb2 user)
        {
            var options = await fido2Service.BeginRegistrationAsync(user);
            await ssoDbContext.SaveChangesAsync();
            CredentialOptionsJson = options.ToJson();
            return Page();
        }
    }
}
