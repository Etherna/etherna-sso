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
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class SecurityKeysModel(
        SignInManager<UserBase> signInManager,
        UserManager<UserBase> userManager)
        : StatusMessagePageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            public string CredentialId { get; set; } = null!;

            [Required]
            [StringLength(Fido2Credential.MaxNicknameLength, MinimumLength = 1)]
            [Display(Name = "Name")]
            public string Nickname { get; set; } = null!;
        }

        // Properties.
        public Fido2Credential Credential { get; set; } = null!;
        public string CredentialIdBase64 { get; set; } = null!;

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public bool IsLastFactor { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync(string credentialId)
        {
            return await LoadAsync(credentialId);
        }

        public async Task<IActionResult> OnPostRenameAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not UserWeb2 userWeb2)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
                return await LoadAsync(Input.CredentialId);

            byte[] credentialIdBytes;
            try { credentialIdBytes = Convert.FromBase64String(Input.CredentialId); }
            catch (FormatException) { return NotFound(); }

            if (!userWeb2.RenameFido2Credential(credentialIdBytes, Input.Nickname))
                return NotFound();

            await userManager.UpdateAsync(user);
            StatusMessage = new StatusMessage("Security key renamed.");
            return RedirectToPage("./TwoFactorAuthentication");
        }

        public async Task<IActionResult> OnPostRemoveAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not UserWeb2 userWeb2)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            byte[] credentialIdBytes;
            try { credentialIdBytes = Convert.FromBase64String(Input.CredentialId); }
            catch (FormatException) { return NotFound(); }

            if (!userWeb2.RemoveFido2Credential(credentialIdBytes))
                return NotFound();

            if (!userWeb2.TwoFactorEnabled)
            {
                if (await signInManager.IsTwoFactorClientRememberedAsync(user))
                    await signInManager.ForgetTwoFactorClientAsync();
                await userManager.SetTwoFactorEnabledAsync(user, false);
            }

            await userManager.UpdateAsync(user);
            StatusMessage = new StatusMessage("Security key removed.");
            return RedirectToPage("./TwoFactorAuthentication");
        }

        // Helpers.
        private async Task<IActionResult> LoadAsync(string credentialId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not UserWeb2 userWeb2)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            byte[] credentialIdBytes;
            try { credentialIdBytes = Convert.FromBase64String(credentialId); }
            catch (FormatException) { return NotFound(); }

            var credential = userWeb2.FindFido2Credential(credentialIdBytes);
            if (credential is null)
                return NotFound();

            Credential = credential;
            CredentialIdBase64 = credentialId;
            IsLastFactor = !userWeb2.IsAuthenticatorAppEnabled && userWeb2.Fido2Credentials.Count() == 1;
            Input = new InputModel { CredentialId = credentialId, Nickname = credential.Nickname };

            return Page();
        }
    }
}
