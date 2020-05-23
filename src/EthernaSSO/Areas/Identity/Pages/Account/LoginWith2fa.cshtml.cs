using Etherna.SSOServer.Domain.Models;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWith2faModel : PageModel
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
        private readonly IEventService eventService;
        private readonly IIdentityServerInteractionService idServerInteractService;
        private readonly ILogger<LoginWith2faModel> logger;
        private readonly SignInManager<User> signInManager;

        // Constructor.
        public LoginWith2faModel(
            IEventService eventService,
            IIdentityServerInteractionService idServerInteractService,
            ILogger<LoginWith2faModel> logger,
            SignInManager<User> signInManager)
        {
            this.eventService = eventService;
            this.idServerInteractService = idServerInteractService;
            this.logger = logger;
            this.signInManager = signInManager;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
        {
            // Init page and validate.
            if (!ModelState.IsValid)
                return Page();

            returnUrl ??= Url.Content("~/");

            // Login.
            //check if we are in the context of an authorization request
            var context = await idServerInteractService.GetAuthorizationContextAsync(returnUrl);

            //check 2fa
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty, StringComparison.InvariantCulture)
                                                       .Replace("-", string.Empty, StringComparison.InvariantCulture);

            var result = await signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

            if (result.Succeeded)
            {
                await eventService.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.Id, user.Username, clientId: context?.ClientId));
                logger.LogInformation($"User with ID '{user.Id}' logged in with 2fa.");

                if (context != null)
                {
                    //we can trust returnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(returnUrl);
                }

                //request for a local page, otherwise user might have clicked on a malicious link - should be logged
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }
    }
}
