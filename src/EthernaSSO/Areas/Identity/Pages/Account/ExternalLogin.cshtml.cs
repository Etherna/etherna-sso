using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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
        }

        // Fields.
        private readonly IClientStore clientStore;
        private readonly IEventService eventService;
        private readonly IIdentityServerInteractionService idServerInteractionService;
        private readonly ILogger<ExternalLoginModel> logger;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        // Constructor.
        public ExternalLoginModel(
            IClientStore clientStore,
            IEventService eventService,
            IIdentityServerInteractionService idServerInteractionService,
            ILogger<ExternalLoginModel> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this.clientStore = clientStore;
            this.eventService = eventService;
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

        public bool DuplicateEmail { get; set; }
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
                throw new Exception("invalid return URL");
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
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = authResult.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            // Get external login info and user.
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
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

                await eventService.RaiseAsync(new UserLoginSuccessEvent(info.LoginProvider, info.ProviderKey, info.ProviderKey, info.Principal.Identity.Name, true, context?.ClientId));
                logger.LogInformation($"{info.Principal.Identity.Name} logged in with {info.LoginProvider} provider.");

                if (context != null)
                {
                    if (await clientStore.IsPkceClientAsync(context.ClientId))
                    {
                        // If the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("/Redirect", returnUrl);
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
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Get the information about the user from the external login provider
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Skip registration if invalid.
            if (ModelState.IsValid)
            {
                // Check for duplicate email.
                if (Input.Email != null)
                {
                    var emailFromUser = await userManager.FindByEmailAsync(Input.Email);
                    if (emailFromUser != null) //if duplicate email
                    {
                        ModelState.AddModelError(string.Empty, "Email already registered.");
                        DuplicateEmail = true;
                        ReturnUrl = returnUrl;
                        ProviderDisplayName = info.ProviderDisplayName;
                        return Page();
                    }
                }

                // Create user.
                var user = Domain.Models.User.CreateManagedWithExternalLogin(
                    new Domain.Models.UserAgg.UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName),
                    Input.Email);

                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // Check if external login is in the context of an OIDC request.
                    var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                    await eventService.RaiseAsync(new UserLoginSuccessEvent(info.LoginProvider, info.ProviderKey, info.ProviderKey, info.Principal.Identity.Name, true, context?.ClientId));
                    logger.LogInformation($"User created an account using {info.LoginProvider} provider.");

                    if (context != null)
                    {
                        if (await clientStore.IsPkceClientAsync(context.ClientId))
                        {
                            // If the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.LoadingPage("/Redirect", returnUrl);
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
