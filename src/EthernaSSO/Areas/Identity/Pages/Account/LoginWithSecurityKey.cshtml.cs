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
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Domain;
using Etherna.SSOServer.Services.Extensions;
using Fido2NetLib;
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
    public class LoginWithSecurityKeyModel(
        IClientStore clientStore,
        IEventDispatcher eventDispatcher,
        IFido2Service fido2Service,
        IIdentityServerInteractionService idServerInteractService,
        ILogger<LoginWithSecurityKeyModel> logger,
        SignInManager<UserBase> signInManager,
        ISsoDbContext ssoDbContext)
        : SsoExitPageModelBase(clientStore)
    {
        // Models.
        public class InputModel
        {
            [Required]
            public string AssertionResponseJson { get; set; } = null!;

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        // Properties.
        public string AssertionOptionsJson { get; set; } = null!;
        public bool HasAuthenticator { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public string? ReturnUrl { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync() ??
                throw new InvalidOperationException("Unable to load two-factor authentication user.");

            if (user is not UserWeb2 userWeb2 || !userWeb2.HasFido2Credentials)
                return RedirectToPage("./LoginWithAuthenticatorApp", new { returnUrl });

            ReturnUrl = returnUrl;
            HasAuthenticator = userWeb2.IsAuthenticatorAppEnabled;

            var options = await fido2Service.BeginAssertionAsync(userWeb2);
            await ssoDbContext.SaveChangesAsync();
            AssertionOptionsJson = options.ToJson();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync() ??
                throw new InvalidOperationException("Unable to load two-factor authentication user.");

            if (user is not UserWeb2 userWeb2)
            {
                ModelState.AddModelError(string.Empty, "Account doesn't support security keys.");
                return await ReloadOptionsAsync(returnUrl, user);
            }

            if (!ModelState.IsValid)
                return await ReloadOptionsAsync(returnUrl, userWeb2);

            AuthenticatorAssertionRawResponse? assertion;
            try
            {
                assertion = System.Text.Json.JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(Input.AssertionResponseJson);
            }
            catch (System.Text.Json.JsonException)
            {
                ModelState.AddModelError(string.Empty, "Invalid security key response.");
                return await ReloadOptionsAsync(returnUrl, userWeb2);
            }
            if (assertion is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid security key response.");
                return await ReloadOptionsAsync(returnUrl, userWeb2);
            }

            try
            {
                await fido2Service.CompleteAssertionAsync(userWeb2, assertion);
                await ssoDbContext.SaveChangesAsync();
            }
            catch (Fido2VerificationException)
            {
                logger.InvalidSecurityKeyAttempt(userWeb2.Id);
                SsoMetrics.RecordLoginAttempt("security_key", "failure");
                ModelState.AddModelError(string.Empty, "Invalid security key response.");
                return await ReloadOptionsAsync(returnUrl, userWeb2);
            }
            catch (InvalidOperationException ex)
            {
                logger.InvalidSecurityKeyAttempt(userWeb2.Id);
                SsoMetrics.RecordLoginAttempt("security_key", "failure");
                ModelState.AddModelError(string.Empty, ex.Message);
                return await ReloadOptionsAsync(returnUrl, userWeb2);
            }

            await signInManager.SignInAsync(userWeb2, isPersistent: true);
            if (Input.RememberMachine)
                await signInManager.RememberTwoFactorClientAsync(userWeb2);

            var context = await idServerInteractService.GetAuthorizationContextAsync(returnUrl, HttpContext.RequestAborted);
            await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(userWeb2, clientId: context?.Client?.ClientId));
            logger.LoggedInWithSecurityKey(userWeb2.Id);
            SsoMetrics.RecordLoginAttempt("security_key", "success");

            return await ContextedRedirectAsync(context, returnUrl);
        }

        // Helpers.
        private async Task<IActionResult> ReloadOptionsAsync(string returnUrl, UserBase user)
        {
            ReturnUrl = returnUrl;
            if (user is UserWeb2 userWeb2)
            {
                HasAuthenticator = userWeb2.IsAuthenticatorAppEnabled;
                var options = await fido2Service.BeginAssertionAsync(userWeb2);
                await ssoDbContext.SaveChangesAsync();
                AssertionOptionsJson = options.ToJson();
            }
            return Page();
        }
    }
}
