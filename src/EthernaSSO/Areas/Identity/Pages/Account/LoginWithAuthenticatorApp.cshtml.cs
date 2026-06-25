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

using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Etherna.DomainEvents;
using Etherna.SSOServer.Configs.Metrics;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWithAuthenticatorAppModel(
        IClientStore clientStore,
        IEventDispatcher eventDispatcher,
        IIdentityServerInteractionService idServerInteractService,
        ILogger<LoginWithAuthenticatorAppModel> logger,
        SignInManager<UserBase> signInManager)
        : SsoExitPageModelBase(clientStore)
    {
        // Models.
        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; } = null!;

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        // Properties.
        public bool HasFido2Credentials { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public string? ReturnUrl { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first.
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync() ??
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");

            HasFido2Credentials = user is UserWeb2 userWeb2 && userWeb2.HasFido2Credentials;
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // Init page and validate.
            if (!ModelState.IsValid)
                return Page();

            returnUrl ??= Url.Content("~/");

            // Login.
            //check 2fa
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync() ??
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty, StringComparison.InvariantCulture)
                                                       .Replace("-", string.Empty, StringComparison.InvariantCulture);

            var result = await signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, true, Input.RememberMachine);

            if (result.Succeeded)
            {
                // Check if we are in the context of an authorization request.
                var context = await idServerInteractService.GetAuthorizationContextAsync(returnUrl, HttpContext.RequestAborted);

                // Raise event and create log.
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(user, clientId: context?.Client?.ClientId));
                logger.LoggedInWith2FA(user.Id);
                SsoMetrics.RecordLoginAttempt("2fa", "success");

                // Identify redirect.
                return await ContextedRedirectAsync(context, returnUrl);
            }
            else if (result.IsLockedOut)
            {
                logger.LockedOutLoginAttempt(user.Id);
                SsoMetrics.RecordLoginAttempt("2fa", "locked_out");
                return RedirectToPage("./Lockout");
            }
            else
            {
                logger.Invalid2FACodeAttempt(user.Id);
                SsoMetrics.RecordLoginAttempt("2fa", "failure");
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }
    }
}
