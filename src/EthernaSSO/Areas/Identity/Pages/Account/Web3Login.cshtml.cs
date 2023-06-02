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
using Duende.IdentityServer.Stores;
using Etherna.DomainEvents;
using Etherna.MongoDB.Driver;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using Etherna.SSOServer.Services.Domain;
using Etherna.SSOServer.Services.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    public class Web3LoginModel : SsoExitPageModelBase
    {
        // Models.
        public class InputModel : IValidatableObject
        {
            // Properties.
            [Display(Name = "Invitation code")]
            public string? InvitationCode { get; set; }

            [Required]
            [RegularExpression(UsernameHelper.UsernameRegex, ErrorMessage = UsernameHelper.UsernameValidationErrorMessage)]
            [Display(Name = "Username")]
            public string Username { get; set; } = default!;

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
        private readonly IIdentityServerInteractionService idServerInteractionService;
        private readonly ILogger<Web3LoginModel> logger;
        private readonly SignInManager<UserBase> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;
        private readonly IUserService userService;
        private readonly IWeb3AuthnService web3AuthnService;

        // Constructor.
        public Web3LoginModel(
            IOptions<ApplicationSettings> applicationSettings,
            IClientStore clientStore,
            IEventDispatcher eventDispatcher,
            IIdentityServerInteractionService idServerInteractionService,
            ILogger<Web3LoginModel> logger,
            SignInManager<UserBase> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager,
            IUserService userService,
            IWeb3AuthnService web3AuthnService)
            : base(clientStore)
        {
            if (applicationSettings is null)
                throw new ArgumentNullException(nameof(applicationSettings));

            this.applicationSettings = applicationSettings.Value;
            this.eventDispatcher = eventDispatcher;
            this.idServerInteractionService = idServerInteractionService;
            this.logger = logger;
            this.signInManager = signInManager;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
            this.userService = userService;
            this.web3AuthnService = web3AuthnService;
        }

        // Properties.
        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public bool DuplicateUsername { get; private set; }
        public string? EtherAddress { get; private set; }
        public bool IsInvitationRequired { get; private set; }
        public string? ReturnUrl { get; private set; }
        public string? Signature { get; private set; }

        // Methods.
        public IActionResult OnGet() =>
            RedirectToPage("./Login");

        public async Task<IActionResult> OnGetRetriveAuthMessageAsync(string etherAddress) =>
            new JsonResult(await web3AuthnService.RetriveAuthnMessageAsync(etherAddress));

        public async Task<IActionResult> OnGetConfirmSignature(string etherAddress, string signature, string? invitationCode, string? returnUrl)
        {
            // Verify signature.
            //get token
            var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            if (token is null)
            {
                ErrorMessage = $"Web3 authentication code for {etherAddress} address not found";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            //check signature
            var verifiedSignature = web3AuthnService.VerifySignature(token.Code, etherAddress, signature);

            if (!verifiedSignature)
            {
                ErrorMessage = $"Invalid signature for web3 authentication";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Initialize page.
            Initialize(etherAddress, signature, returnUrl);

            // Sign in user with ethereum address if already has an account.
            // Search for both Web2 accounts with ether login, and for Web3 accounts.
            var cursor = await ssoDbContext.Users.FindAsync<UserBase>(Builders<UserBase>.Filter.Or(
                Builders<UserBase>.Filter.Eq(u => u.EtherAddress, etherAddress),    //UserWeb3
                Builders<UserBase>.Filter.Eq("EtherLoginAddress", etherAddress)));  //UserWeb2
            var user = await cursor.FirstOrDefaultAsync();

            if (user != null)
            {
                // Check if user is locked out.
                if (await userManager.IsLockedOutAsync(user))
                    return RedirectToPage("./Lockout");

                // Login.
                await signInManager.SignInAsync(user, true);

                // Delete used token.
                await ssoDbContext.Web3LoginTokens.DeleteAsync(token);

                // Check if we are in the context of an authorization request.
                var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                // Rise event and create log.
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(
                    user,
                    clientId: context?.Client?.ClientId,
                    provider: "web3",
                    providerUserId: etherAddress));
                logger.LoggedInWithWeb3(user.Id);

                // Identify redirect.
                return await ContextedRedirectAsync(context, returnUrl);
            }

            // If user does not have an account, then ask him to create one.
            Input = new InputModel
            {
                InvitationCode = invitationCode,
            };
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string etherAddress, string signature, string? returnUrl)
        {
            // Verify signature.
            //get token
            var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            if (token is null)
            {
                ErrorMessage = $"Web3 authentication code for {etherAddress} address not found";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            //check signature
            var verifiedSignature = web3AuthnService.VerifySignature(token.Code, etherAddress, signature);
            if (!verifiedSignature)
            {
                ErrorMessage = $"Invalid signature for web3 authentication";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Init page and validate.
            Initialize(etherAddress, signature, returnUrl);
            if (!ModelState.IsValid)
                return Page();

            // Register user.
            var (errors, user) = await userService.RegisterWeb3UserAsync(
                Input.Username,
                etherAddress,
                Input.InvitationCode);

            // Post-registration actions.
            if (user is not null)
            {
                // Login.
                await signInManager.SignInAsync(user, true);

                // Check if we are in the context of an authorization request.
                var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                // Rise event and create log.
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(
                    user,
                    clientId: context?.Client?.ClientId,
                    provider: "web3",
                    providerUserId: etherAddress));
                logger.CreatedAccountWithWeb3(user.Id);

                // Redirect to add verified email page.
                return RedirectToPage("SetVerifiedEmail", new { returnUrl });
            }

            // Report errors and show page again.
            foreach (var (errorKey, errorMessage) in errors)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                if (errorKey == UserService.DuplicateUsernameErrorKey)
                    DuplicateUsername = true;
            }
            return Page();
        }

        // Helpers.
        private void Initialize(string etherAddress, string signature, string? returnUrl)
        {
            EtherAddress = etherAddress;
            IsInvitationRequired = applicationSettings.RequireInvitation;
            ReturnUrl = returnUrl ?? Url.Content("~/");
            Signature = signature;
        }
    }
}
