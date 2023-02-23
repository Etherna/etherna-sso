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

using Duende.IdentityServer.Services;
using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using Etherna.SSOServer.Services.Domain;
using Etherna.SSOServer.Services.Settings;
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
            [RegularExpression(UsernameHelper.UsernameRegex, ErrorMessage = UsernameHelper.UsernameValidationErrorMessage)]
            [Display(Name = "Username")]
            public string Username { get; set; } = default!;

            [Display(Name = "Invitation code")]
            public string? InvitationCode { get; set; }

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
                if (validationContext is null)
                    throw new ArgumentNullException(nameof(validationContext));

                var appSettings = (IOptions<ApplicationSettings>)validationContext.GetService(typeof(IOptions<ApplicationSettings>))!;
                if (appSettings.Value.RequireInvitation && string.IsNullOrWhiteSpace(InvitationCode))
                {
                    yield return new ValidationResult(
                        "Invitation code is required",
                        new[] { nameof(InvitationCode) });
                }
            }
        }

        // Fields.
        private readonly ApplicationSettings applicationSettings;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IIdentityServerInteractionService idServerInteractService;
        private readonly ILogger<RegisterModel> logger;
        private readonly SignInManager<UserBase> signInManager;
        private readonly IUserService userService;

        // Constructor.
        public RegisterModel(
            IOptions<ApplicationSettings> applicationSettings,
            IEventDispatcher eventDispatcher,
            IIdentityServerInteractionService idServerInteractService,
            ILogger<RegisterModel> logger,
            SignInManager<UserBase> signInManager,
            IUserService userService)
        {
            if (applicationSettings is null)
                throw new ArgumentNullException(nameof(applicationSettings));

            this.applicationSettings = applicationSettings.Value;
            this.eventDispatcher = eventDispatcher;
            this.idServerInteractService = idServerInteractService;
            this.logger = logger;
            this.signInManager = signInManager;
            this.userService = userService;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public List<AuthenticationScheme> ExternalLogins { get; } = new List<AuthenticationScheme>();
        public bool IsInvitationRequired { get; private set; }
        public string? ReturnUrl { get; private set; }
        public Web3LoginPartialModel Web3LoginPartialModel { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string? invitationCode, string? returnUrl = null) =>
            await InitializeAsync(invitationCode, returnUrl);

        public async Task<IActionResult> OnPostAsync(string? invitationCode, string? returnUrl = null)
        {
            // Init page and validate.
            await InitializeAsync(invitationCode, returnUrl);
            if (!ModelState.IsValid)
                return Page();

            // Register user.
            var (errors, user) = await userService.RegisterWeb2UserAsync(
                Input.Username,
                Input.Password,
                Input.InvitationCode);

            // Post-registration actions.
            if (user is not null)
            {
                // Check if we are in the context of an authorization request.
                var context = await idServerInteractService.GetAuthorizationContextAsync(returnUrl);

                // Login.
                await signInManager.SignInAsync(user, true);

                // Rise event and create log.
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(user, clientId: context?.Client?.ClientId));
                logger.CreatedAccountWithPassword(user.Id);

                // Redirect to add verified email page.
                return RedirectToPage("SetVerifiedEmail", new { returnUrl });
            }

            // If we got this far, something failed, redisplay form printing errors.
            foreach (var (_, errorMessage) in errors)
                ModelState.AddModelError(string.Empty, errorMessage);

            return Page();
        }

        // Helpers.
        private async Task InitializeAsync(string? invitationCode, string? returnUrl)
        {
            //clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //load data
            ExternalLogins.AddRange(await signInManager.GetExternalAuthenticationSchemesAsync());
            Input ??= new InputModel();
            Input.InvitationCode ??= invitationCode;
            IsInvitationRequired = applicationSettings.RequireInvitation;
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
