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
using Etherna.SSOServer.Services.Domain;
using Etherna.SSOServer.Services.Settings;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    public class Web3LoginModel : PageModel
    {
        // Models.
        public class InputModel : IValidatableObject
        {
            // Properties.
            [EmailAddress]
            [Display(Name = "Email (optional)")]
            public string? Email { get; set; }

            [Display(Name = "Invitation code")]
            public string? InvitationCode { get; set; }

            public bool IsInvitationRequired { get; set; }

            [Required]
            [RegularExpression(UserBase.UsernameRegex, ErrorMessage = "Allowed characters are a-z, A-Z, 0-9, _. Permitted length is between 5 and 20.")]
            [Display(Name = "Username")]
            public string Username { get; set; } = default!;

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
        private readonly IIdentityServerInteractionService idServerInteractionService;
        private readonly ILogger<ExternalLoginModel> logger;
        private readonly SignInManager<UserBase> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;
        private readonly IWeb3AuthnService web3AuthnService;

        // Constructor.
        public Web3LoginModel(
            IOptions<ApplicationSettings> applicationSettings,
            IClientStore clientStore,
            IEventDispatcher eventDispatcher,
            IIdentityServerInteractionService idServerInteractionService,
            ILogger<ExternalLoginModel> logger,
            SignInManager<UserBase> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager,
            IWeb3AuthnService web3AuthnService)
        {
            if (applicationSettings is null)
                throw new ArgumentNullException(nameof(applicationSettings));

            this.applicationSettings = applicationSettings.Value;
            this.clientStore = clientStore;
            this.eventDispatcher = eventDispatcher;
            this.idServerInteractionService = idServerInteractionService;
            this.logger = logger;
            this.signInManager = signInManager;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
            this.web3AuthnService = web3AuthnService;
        }

        // Properties.
        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public bool DuplicateEmail { get; set; }
        public bool DuplicateUsername { get; set; }
        public string? EtherAddress { get; private set; }
        public string? ReturnUrl { get; set; }
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

                // Sign in.
                await signInManager.SignInAsync(user, true);

                // Delete used token.
                await ssoDbContext.Web3LoginTokens.DeleteAsync(token);

                // Check if external login is in the context of an OIDC request.
                var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                // Rise event and create log.
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(
                    user,
                    clientId: context?.Client?.ClientId,
                    provider: "web3",
                    providerUserId: etherAddress));
                logger.LogInformation($"{etherAddress} logged in with web3.");

                // Identify redirect.
                if (context?.Client != null)
                {
                    if (await clientStore.IsPkceClientAsync(context.Client.ClientId))
                    {
                        // If the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("/Redirect", returnUrl!);
                    }
                }

                return Redirect(returnUrl);
            }

            // If user does not have an account, then ask him to create one.
            Input = new InputModel
            {
                InvitationCode = invitationCode,
                IsInvitationRequired = applicationSettings.RequireInvitation,
            };
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string etherAddress, string signature, string? returnUrl)
        {
            // Initialize page.
            Initialize(etherAddress, signature, returnUrl);

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

            // Skip registration if invalid.
            if (!ModelState.IsValid)
                return Page();

            // Check for duplicate username.
            var userByUsername = await userManager.FindByNameAsync(Input.Username);
            if (userByUsername != null) //if duplicate username
            {
                ModelState.AddModelError(string.Empty, "Username already registered.");
                DuplicateUsername = true;
            }

            // Check for duplicate email.
            if (Input.Email != null)
            {
                var userByEmail = await userManager.FindByEmailAsync(Input.Email);
                if (userByEmail != null) //if duplicate email
                {
                    ModelState.AddModelError(string.Empty, "Email already registered.");
                    DuplicateEmail = true;
                }
            }

            // Duplicate elements error.
            if (DuplicateUsername || DuplicateEmail)
                return Page();

            // Verify invitation code.
            UserBase? invitedByUser = null;
            if (Input.InvitationCode is not null)
            {
                var invitation = await ssoDbContext.Invitations.TryFindOneAsync(i => i.Code == Input.InvitationCode);
                if (invitation is null)
                {
                    ModelState.AddModelError(string.Empty, "Invitation is not valid.");
                    return Page();
                }

                // Delete used invitation.
                await ssoDbContext.Invitations.DeleteAsync(invitation);

                // Get inviting user.
                invitedByUser = invitation.Owner;
            }

            // Create user.
            var user = new UserWeb3(
                etherAddress,
                Input.Username,
                Input.Email,
                invitedByUser);

            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                // Login.
                await signInManager.SignInAsync(user, true);

                // Check if external login is in the context of an OIDC request.
                var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                // Rise event and create log.
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(
                    user,
                    clientId: context?.Client?.ClientId,
                    provider: "web3",
                    providerUserId: etherAddress));
                logger.LogInformation($"User created an account using web3 address.");

                // Identify redirect.
                if (context?.Client != null)
                {
                    if (await clientStore.IsPkceClientAsync(context.Client.ClientId))
                    {
                        // If the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("/Redirect", returnUrl!);
                    }
                }

                return Redirect(returnUrl);
            }

            // Report errors and show page again.
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }

        // Helpers.
        private void Initialize(string etherAddress, string signature, string? returnUrl)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            EtherAddress = etherAddress;
            Signature = signature;
        }
    }
}
