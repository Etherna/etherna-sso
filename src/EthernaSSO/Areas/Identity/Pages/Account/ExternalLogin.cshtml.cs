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
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [EmailAddress]
            [Display(Name = "Email (optional)")]
            public string? Email { get; set; }

            [Required]
            [RegularExpression(Domain.Models.User.UsernameRegex, ErrorMessage = "Allowed characters are a-z, A-Z, 0-9, _. Permitted length is between 5 and 20.")]
            [Display(Name = "Username")]
            public string Username { get; set; } = default!;
        }

        // Fields.
        private readonly IClientStore clientStore;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IIdentityServerInteractionService idServerInteractionService;
        private readonly ILogger<ExternalLoginModel> logger;
        private readonly SignInManager<User> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<User> userManager;

        // Constructor.
        public ExternalLoginModel(
            IClientStore clientStore,
            IEventDispatcher eventDispatcher,
            IIdentityServerInteractionService idServerInteractionService,
            ILogger<ExternalLoginModel> logger,
            SignInManager<User> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<User> userManager)
        {
            this.clientStore = clientStore;
            this.eventDispatcher = eventDispatcher;
            this.idServerInteractionService = idServerInteractionService;
            this.logger = logger;
            this.signInManager = signInManager;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Properties.
        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public bool DuplicateEmail { get; set; }
        public bool DuplicateUsername { get; set; }
        public string? ProviderDisplayName { get; set; }
        public string? ReturnUrl { get; set; }

        // Methods.
        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Validate returnUrl - either it is a valid OIDC URL or back to a local page.
            if (Url.IsLocalUrl(returnUrl) == false && idServerInteractionService.IsValidReturnUrl(returnUrl) == false)
            {
                //user might have clicked on a malicious link - should be logged
                throw new InvalidOperationException("invalid return URL");
            }

            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Read external identity from the temporary cookie.
            var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (authResult?.Succeeded != true)
            {
                ErrorMessage = "External authentication error";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Log claims.
            if (logger.IsEnabled(LogLevel.Debug) && authResult.Principal is not null)
            {
                var externalClaims = authResult.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            // Get external login info and user.
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info is null || info.Principal.Identity is null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                // Check if external login is in the context of an OIDC request.
                var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                // Rise event and create log.
                var user = await ssoDbContext.Users.QueryElementsAsync(elements =>
                    elements.FirstOrDefaultAsync(u => u.Logins.Any(
                        l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey)));
                await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(
                    user,
                    clientId: context?.Client?.ClientId,
                    provider: info.LoginProvider,
                    providerUserId: info.ProviderKey));
                logger.LogInformation($"{info.Principal.Identity.Name} logged in with {info.LoginProvider} provider.");

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

            // Check if user is locked out.
            else if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }

            // If the user does not have an account, then ask him to create an account.
            else
            {
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                Input = new InputModel
                {
                    Email = info.Principal.HasClaim(c => c.Type == ClaimTypes.Email) ?
                        info.Principal.FindFirstValue(ClaimTypes.Email) : null,
                    Username = info.Principal.HasClaim(c => c.Type == ClaimTypes.Name) ?
                        info.Principal.FindFirstValue(ClaimTypes.Name) : ""
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Get the information about the user from the external login provider
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info is null || info.Principal.Identity is null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Skip registration if invalid.
            if (ModelState.IsValid)
            {
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
                {
                    ReturnUrl = returnUrl;
                    ProviderDisplayName = info.ProviderDisplayName;
                    return Page();
                }

                // Create user.
                var user = Domain.Models.User.CreateManagedWithExternalLogin(
                    new Domain.Models.UserAgg.UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName),
                    Input.Username,
                    Input.Email);

                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // Login.
                    await signInManager.SignInAsync(user, false);

                    // Check if external login is in the context of an OIDC request.
                    var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                    // Rise event and create log.
                    await eventDispatcher.DispatchAsync(new UserLoginSuccessEvent(
                        user,
                        clientId: context?.Client?.ClientId,
                        provider: info.LoginProvider,
                        providerUserId: info.ProviderKey));
                    logger.LogInformation($"User created an account using {info.LoginProvider} provider.");

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

                // Report errors.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Show page again if there was errors.
            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
