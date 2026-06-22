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
using Etherna.SSOServer.Services.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class AddAuthenticatorAppModel(
        IOptions<ApplicationOptions> applicationSettings,
        ILogger<AddAuthenticatorAppModel> logger,
        UrlEncoder urlEncoder,
        UserManager<UserBase> userManager)
        : StatusMessagePageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Verification Code")]
            public string Code { get; set; } = null!;
        }

        // Fields.
        private readonly ApplicationOptions applicationOptions = applicationSettings.Value;

        // Properties.
        public string AuthenticatorUri { get; set; } = null!;

#pragma warning disable CA1819 // Properties should not return arrays
        [TempData]
        public string[] RecoveryCodes { get; set; } = null!;
#pragma warning restore CA1819 // Properties should not return arrays

        public string SharedKey { get; set; } = null!;

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not UserWeb2 userWeb2)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            // Strip spaces and hypens
            var verificationCode = Input.Code.Replace(" ", string.Empty, StringComparison.InvariantCulture)
                                             .Replace("-", string.Empty, StringComparison.InvariantCulture);

            var is2faTokenValid = await userManager.VerifyTwoFactorTokenAsync(
                user, userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            // The key is already stored from the GET; the app becomes an active second factor only now,
            // once the user has proven they configured it by entering a valid verification code.
            userWeb2.EnableAuthenticatorApp();
            await userManager.UpdateAsync(user);

            var userId = await userManager.GetUserIdAsync(user);
            logger.Enabled2FAWithAuthApp(userId);

            StatusMessage = new StatusMessage("Your authenticator app has been verified.");

            if (await userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10) ?? throw new InvalidOperationException();
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }
            else
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }
        }

        // Helpers.
        private async Task LoadSharedKeyAndQrCodeUriAsync(UserBase user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await userManager.GetAuthenticatorKeyAsync(user) ?? throw new InvalidOperationException();
            }

            SharedKey = FormatKey(unformattedKey);

            var username = await userManager.GetUserNameAsync(user) ?? throw new InvalidOperationException();
            AuthenticatorUri = GenerateQrCodeUri(applicationOptions.DisplayName, username, unformattedKey);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey[currentPosition..]);
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string serviceName, string username, string unformattedKey) =>
            $"otpauth://totp/{urlEncoder.Encode(username)}?secret={unformattedKey}&issuer={urlEncoder.Encode(serviceName)}&digits=6";
    }
}
