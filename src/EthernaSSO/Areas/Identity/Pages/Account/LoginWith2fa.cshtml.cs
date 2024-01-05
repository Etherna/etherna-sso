// Copyright 2021-present Etherna Sa
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

using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
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
    public class LoginWith2faModel : SsoExitPageModelBase
    {
        // Models.
        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; } = default!;

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        // Fields.
        private readonly IEventDispatcher eventDispatcher;
        private readonly IIdentityServerInteractionService idServerInteractService;
        private readonly ILogger<LoginWith2faModel> logger;
        private readonly SignInManager<UserBase> signInManager;

        // Constructor.
        public LoginWith2faModel(
            IClientStore clientStore,
            IEventDispatcher eventDispatcher,
            IIdentityServerInteractionService idServerInteractService,
            ILogger<LoginWith2faModel> logger,
            SignInManager<UserBase> signInManager)
            : base(clientStore)
        {
            this.eventDispatcher = eventDispatcher;
            this.idServerInteractService = idServerInteractService;
            this.logger = logger;
            this.signInManager = signInManager;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string? ReturnUrl { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first.
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync() ??
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
#pragma warning restore IDE0059 // Unnecessary assignment of a value

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
                var context = await idServerInteractService.GetAuthorizationContextAsync(returnUrl);

                // Rise event and create log.
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(user, clientId: context?.Client?.ClientId));
                logger.LoggedInWith2FA(user.Id);

                // Identify redirect.
                return await ContextedRedirectAsync(context, returnUrl);
            }
            else if (result.IsLockedOut)
            {
                logger.LockedOutLoginAttempt(user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                logger.Invalid2FACodeAttempt(user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }
    }
}
