//   Copyright 2021-present Etherna Sagl
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

using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : SsoExitPageModelBase
    {
        // Models.
        public class InputModel
        {
            [Required]
            [Display(Name = "Username or email")]
            public string UsernameOrEmail { get; set; } = default!;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = default!;
        }

        // Fields.
        private readonly IEventDispatcher eventDispatcher;
        private readonly IIdentityServerInteractionService idServerInteractionService;
        private readonly ILogger<LoginModel> logger;
        private readonly SignInManager<UserBase> signInManager;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public LoginModel(
            IClientStore clientStore,
            IEventDispatcher eventDispatcher,
            IIdentityServerInteractionService idServerInteractionService,
            ILogger<LoginModel> logger,
            SignInManager<UserBase> signInManager,
            UserManager<UserBase> userManager)
            : base(clientStore)
        {
            this.eventDispatcher = eventDispatcher;
            this.idServerInteractionService = idServerInteractionService;
            this.logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        // Properties.
        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public List<AuthenticationScheme> ExternalLogins { get; } = new List<AuthenticationScheme>();
        public string? InvitationCode { get; set; }
        public string? ReturnUrl { get; set; }
        public Web3LoginPartialModel Web3LoginPartialModel { get; set; } = default!;

        // Methods.
        public async Task OnGetAsync(string? invitationCode = null, string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            await InitializeAsync(invitationCode, returnUrl);
        }

        public async Task<IActionResult> OnPostAsync(string? invitationCode = null, string? returnUrl = null)
        {
            // Init page and validate.
            await InitializeAsync(invitationCode, returnUrl);
            if (!ModelState.IsValid)
                return Page();

            // Login.
            //check if we are in the context of an authorization request
            var context = await idServerInteractionService.GetAuthorizationContextAsync(ReturnUrl);

            //find user
            var user = Input.UsernameOrEmail.Contains('@', StringComparison.InvariantCulture) ? //if is email
                await userManager.FindByEmailAsync(Input.UsernameOrEmail) :
                await userManager.FindByNameAsync(Input.UsernameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            //validate login
            var result = await signInManager.PasswordSignInAsync(user, Input.Password, true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Rise event and create log.
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(user, clientId: context?.Client?.ClientId));
                logger.LoggedInWithPassword(user.Id);

                // Identify redirect.
                return await ContextedRedirectAsync(context, returnUrl);
            }

            else if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl });
            }

            else if (result.IsLockedOut)
            {
                logger.LockedOutLoginAttempt(user.Id);
                return RedirectToPage("./Lockout");
            }

            else
            {
                await eventDispatcher.DispatchAsync(new UserLoginFailureEvent(Input.UsernameOrEmail, "invalid credentials", clientId: context?.Client?.ClientId));
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // Helpers.
        private async Task InitializeAsync(string? invitationCode, string? returnUrl)
        {
            //clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //load data
            ExternalLogins.AddRange(await signInManager.GetExternalAuthenticationSchemesAsync());
            InvitationCode = invitationCode;
            ReturnUrl = returnUrl ?? Url.Content("~/");

            //init partial view models
            Web3LoginPartialModel = new Web3LoginPartialModel()
            {
                InvitationCode = InvitationCode,
                ReturnUrl = ReturnUrl
            };
        }
    }
}
