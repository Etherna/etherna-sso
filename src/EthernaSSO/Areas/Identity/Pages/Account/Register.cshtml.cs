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
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [RegularExpression(Domain.Models.User.UsernameRegex, ErrorMessage = "Allowed characters are a-z, A-Z, 0-9, _. Permitted length is between 5 and 20.")]
            [Display(Name = "Username")]
            public string Username { get; set; } = default!;

            [EmailAddress]
            [Display(Name = "Email (optional, needed for password recovery)")]
            public string? Email { get; set; } = default!;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = default!;
        }

        // Fields.
        private readonly IClientStore clientStore;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IIdentityServerInteractionService idServerInteractService;
        private readonly ILogger<RegisterModel> logger;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        // Constructor.
        public RegisterModel(
            IClientStore clientStore,
            IEventDispatcher eventDispatcher,
            IIdentityServerInteractionService idServerInteractService,
            ILogger<RegisterModel> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this.clientStore = clientStore;
            this.eventDispatcher = eventDispatcher;
            this.idServerInteractService = idServerInteractService;
            this.logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public List<AuthenticationScheme> ExternalLogins { get; } = new List<AuthenticationScheme>();
        public string? ReturnUrl { get; set; }

        // Methods.
        public async Task OnGetAsync(string? returnUrl = null)
        {
            await Initialize();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // Init page and validate.
            returnUrl ??= Url.Content("~/");
            ExternalLogins.AddRange(await signInManager.GetExternalAuthenticationSchemesAsync());

            if (!ModelState.IsValid)
                return await InitializedPage();

            // Register new user.
            //check if we are in the context of an authorization request
            var context = await idServerInteractService.GetAuthorizationContextAsync(returnUrl);

            var user = Domain.Models.User.CreateManagedWithUsername(Input.Username, email: Input.Email);
            var result = await userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Login.
                await signInManager.SignInAsync(user, false);

                // Rise event and create log.
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(user, clientId: context?.Client?.ClientId));
                logger.LogInformation("User created a new account with password.");

                // Identify redirect.
                if (context?.Client != null)
                {
                    if (await clientStore.IsPkceClientAsync(context.Client.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("/Redirect", returnUrl!);
                    }

                    //we can trust returnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(returnUrl);
                }

                //request for a local page, otherwise user might have clicked on a malicious link - should be logged
                return LocalRedirect(returnUrl);
            }

            // If we got this far, something failed, redisplay form printing errors.
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return await InitializedPage();
        }

        // Helpers.
        private async Task Initialize()
        {
            //clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //load data
            ExternalLogins.AddRange(await signInManager.GetExternalAuthenticationSchemesAsync());
        }

        private async Task<IActionResult> InitializedPage()
        {
            await Initialize();
            return Page();
        }
    }
}
