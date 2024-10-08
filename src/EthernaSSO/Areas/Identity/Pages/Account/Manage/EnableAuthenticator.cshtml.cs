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
using Etherna.SSOServer.Services.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class EnableAuthenticatorModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Verification Code")]
            public string Code { get; set; } = default!;
        }

        // Fields.
        private readonly ApplicationSettings applicationSettings;
        private readonly ILogger<EnableAuthenticatorModel> logger;
        private readonly UrlEncoder urlEncoder;
        private readonly UserManager<UserBase> userManager;

        public EnableAuthenticatorModel(
            IOptions<ApplicationSettings> applicationSettings,
            ILogger<EnableAuthenticatorModel> logger,
            UrlEncoder urlEncoder,
            UserManager<UserBase> userManager)
        {
            ArgumentNullException.ThrowIfNull(applicationSettings, nameof(applicationSettings));

            this.applicationSettings = applicationSettings.Value;
            this.logger = logger;
            this.urlEncoder = urlEncoder;
            this.userManager = userManager;
        }

        public string AuthenticatorUri { get; set; } = default!;

#pragma warning disable CA1819 // Properties should not return arrays
        [TempData]
        public string[] RecoveryCodes { get; set; } = default!;
#pragma warning restore CA1819 // Properties should not return arrays

        public string SharedKey { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

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
            if (user == null)
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

            await userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await userManager.GetUserIdAsync(user);
            logger.Enabled2FAWithAuthApp(userId);

            StatusMessage = "Your authenticator app has been verified.";

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
            AuthenticatorUri = GenerateQrCodeUri(applicationSettings.DisplayName, username, unformattedKey);
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
            $"otpauth://totp/{urlEncoder.Encode(serviceName)}%20({urlEncoder.Encode(username)})?secret={unformattedKey}&issuer={urlEncoder.Encode(serviceName)}&digits=6";
    }
}
