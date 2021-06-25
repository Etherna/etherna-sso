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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using Etherna.SSOServer.Services.Domain;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    public class Web3LoginModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [EmailAddress]
            [Display(Name = "Email (optional)")]
            public string? Email { get; set; }
        }

        // Fields.
        private readonly IClientStore clientStore;
        private readonly IEventService eventService;
        private readonly IIdentityServerInteractionService idServerInteractionService;
        private readonly ILogger<ExternalLoginModel> logger;
        private readonly SignInManager<User> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<User> userManager;
        private readonly IWeb3AuthnService web3AuthnService;

        // Constructor.
        public Web3LoginModel(
            IClientStore clientStore,
            IEventService eventService,
            IIdentityServerInteractionService idServerInteractionService,
            ILogger<ExternalLoginModel> logger,
            SignInManager<User> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<User> userManager,
            IWeb3AuthnService web3AuthnService)
        {
            this.clientStore = clientStore;
            this.eventService = eventService;
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
        public string? EtherAddress { get; private set; }
        public string? ReturnUrl { get; set; }
        public string? Signature { get; private set; }

        // Methods.
        public IActionResult OnGet()
            => RedirectToPage("./Login");

        public async Task<IActionResult> OnGetRetriveAuthMessageAsync(string etherAddress) =>
            new JsonResult(await web3AuthnService.RetriveAuthnMessageAsync(etherAddress));

        public async Task<IActionResult> OnGetConfirmSignature(string etherAddress, string signature, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

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

            // Sign in user with web3 if already has an account.
            var user = await ssoDbContext.Users.TryFindOneAsync(u => u.EtherLoginAddress == etherAddress);
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

                await eventService.RaiseAsync(new UserLoginSuccessEvent("web3", etherAddress, etherAddress, "web3", true, context?.Client?.ClientId));
                logger.LogInformation($"{etherAddress} logged in with web3.");

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
            else
            {
                ReturnUrl = returnUrl;
                EtherAddress = etherAddress;
                Signature = signature;

                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string etherAddress, string signature, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ReturnUrl = returnUrl;
            EtherAddress = etherAddress;
            Signature = signature;

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

            // Check for duplicate email.
            if (Input.Email != null)
            {
                var emailFromUser = await userManager.FindByEmailAsync(Input.Email);
                if (emailFromUser != null) //if duplicate email
                {
                    ModelState.AddModelError(string.Empty, "Email already registered.");
                    DuplicateEmail = true;

                    return Page();
                }
            }

            // Create user.
            var user = Domain.Models.User.CreateManagedWithEtherLoginAddress(etherAddress, Input.Email);

            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                // Check if external login is in the context of an OIDC request.
                var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                await eventService.RaiseAsync(new UserLoginSuccessEvent("web3", etherAddress, etherAddress, "web3", true, context?.Client?.ClientId));
                logger.LogInformation($"User created an account using web3 address.");

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
    }
}
