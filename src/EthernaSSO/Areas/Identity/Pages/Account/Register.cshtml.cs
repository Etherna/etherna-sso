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
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using Etherna.SSOServer.Services.Settings;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        public class InputModel : IValidatableObject
        {
            // Properties.
            [Required]
            [RegularExpression(UserBase.UsernameRegex, ErrorMessage = "Allowed characters are a-z, A-Z, 0-9, _. Permitted length is between 5 and 20.")]
            [Display(Name = "Username")]
            public string Username { get; set; } = default!;

            [EmailAddress]
            [Display(Name = "Email (optional, needed for password recovery)")]
            public string? Email { get; set; } = default!;

            [Display(Name = "Invitation code")]
            public string? InvitationCode { get; set; }

            public bool IsInvitationRequired { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = default!;

            // Methods.
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (IsInvitationRequired && string.IsNullOrWhiteSpace(InvitationCode))
                {
                    yield return new ValidationResult(
                        "Invitation code is required",
                        new[] { nameof(InvitationCode) });
                }
            }
        }

        // Fields.
        private readonly ApplicationSettings applicationSettings;
        private readonly IClientStore clientStore;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IIdentityServerInteractionService idServerInteractService;
        private readonly ILogger<RegisterModel> logger;
        private readonly SignInManager<UserBase> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public RegisterModel(
            IOptions<ApplicationSettings> applicationSettings,
            IClientStore clientStore,
            IEventDispatcher eventDispatcher,
            IIdentityServerInteractionService idServerInteractService,
            ILogger<RegisterModel> logger,
            SignInManager<UserBase> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            if (applicationSettings is null)
                throw new ArgumentNullException(nameof(applicationSettings));

            this.applicationSettings = applicationSettings.Value;
            this.clientStore = clientStore;
            this.eventDispatcher = eventDispatcher;
            this.idServerInteractService = idServerInteractService;
            this.logger = logger;
            this.signInManager = signInManager;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public List<AuthenticationScheme> ExternalLogins { get; } = new List<AuthenticationScheme>();
        public string? ReturnUrl { get; set; }
        public Web3LoginPartialModel Web3LoginPartialModel { get; set; } = default!;

        // Methods.
        public async Task OnGetAsync(string? invitationCode, string? returnUrl = null) =>
            await InitializeAsync(invitationCode, returnUrl);

        public async Task<IActionResult> OnPostAsync(string? invitationCode, string? returnUrl = null)
        {
            // Init page and validate.
            await InitializeAsync(invitationCode, returnUrl);

            if (!ModelState.IsValid)
                return Page();

            // Verify invitation code.
            UserBase? invitedByUser = null;
            if (Input.InvitationCode is not null)
            {
                var invitation = await ssoDbContext.Invitations.TryFindOneAsync(i => i.Code == Input.InvitationCode);
                if (invitation is null || !invitation.IsAlive)
                {
                    ModelState.AddModelError(string.Empty, "Invitation is not valid.");
                    return Page();
                }

                // Delete used invitation.
                if (invitation.IsSingleUse)
                    await ssoDbContext.Invitations.DeleteAsync(invitation);

                // Get inviting user.
                invitedByUser = invitation.Emitter;
            }

            // Register new user.
            //check if we are in the context of an authorization request
            var context = await idServerInteractService.GetAuthorizationContextAsync(returnUrl);

            var user = new UserWeb2(Input.Username, Input.Email, invitedByUser);
            var result = await userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Login.
                await signInManager.SignInAsync(user, true);

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
                ModelState.AddModelError(error.Code, error.Description);

            return Page();
        }

        // Helpers.
        private async Task InitializeAsync(string? invitationCode, string? returnUrl)
        {
            //clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //load data
            ExternalLogins.AddRange(await signInManager.GetExternalAuthenticationSchemesAsync());
            if (Input is null) Input = new InputModel();
            Input.InvitationCode ??= invitationCode;
            Input.IsInvitationRequired = applicationSettings.RequireInvitation;
            ReturnUrl = returnUrl ?? Url.Content("~/");

            //init partial view models
            Web3LoginPartialModel = new Web3LoginPartialModel()
            {
                InvitationCode = Input.InvitationCode,
                ReturnUrl = ReturnUrl
            };
        }
    }
}
