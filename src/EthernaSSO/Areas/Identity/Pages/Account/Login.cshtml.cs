using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using IdentityServer4.Events;
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
    public class LoginModel : PageModel
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

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        // Fields.
        private readonly IClientStore clientStore;
        private readonly IEventService eventService;
        private readonly IIdentityServerInteractionService idServerInteractService;
        private readonly ILogger<LoginModel> logger;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        // Constructor.
        public LoginModel(
            IClientStore clientStore,
            IEventService eventService,
            IIdentityServerInteractionService idServerInteractService,
            ILogger<LoginModel> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this.clientStore = clientStore;
            this.eventService = eventService;
            this.idServerInteractService = idServerInteractService;
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
        public string? ReturnUrl { get; set; }

        // Methods.
        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins.AddRange(await signInManager.GetExternalAuthenticationSchemesAsync());
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // Init page and validate.
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            // Login.
            //check if we are in the context of an authorization request
            var context = await idServerInteractService.GetAuthorizationContextAsync(ReturnUrl);

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
            var result = await signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await eventService.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.Id, user.Username, clientId: context?.Client?.ClientId));
                logger.LogInformation("User logged in.");

                if (context?.Client != null)
                {
                    if (await clientStore.IsPkceClientAsync(context.Client.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("/Redirect", ReturnUrl);
                    }

                    //we can trust returnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(ReturnUrl);
                }

                //request for a local page, otherwise user might have clicked on a malicious link - should be logged
                return LocalRedirect(ReturnUrl);
            }

            else if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl, Input.RememberMe });
            }

            else if (result.IsLockedOut)
            {
                logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            else
            {
                await eventService.RaiseAsync(new UserLoginFailureEvent(Input.UsernameOrEmail, "invalid credentials", clientId: context?.Client?.ClientId));
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }
    }
}
