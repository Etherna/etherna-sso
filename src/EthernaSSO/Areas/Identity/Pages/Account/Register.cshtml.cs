using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
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
            [RegularExpression(Domain.Models.User.UsernameRegex)]
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
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        // Constructor.
        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public List<AuthenticationScheme> ExternalLogins { get; } = new List<AuthenticationScheme>();
        public string? ReturnUrl { get; set; }

        // Methods.
        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins.AddRange(await _signInManager.GetExternalAuthenticationSchemesAsync());
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (Input is null)
                throw new InvalidOperationException();

            // Init page and validate.
            returnUrl ??= Url.Content("~/");
            ExternalLogins.AddRange(await _signInManager.GetExternalAuthenticationSchemesAsync());

            if (!ModelState.IsValid)
                return Page();

            // Register new user.
            //geneate new managed account
            var managedAccount = new EtherAccount("0xD12C40D24C4307B825BFa150b1E578382488ca97"/*sample*/);

            //create user
            var user = Domain.Models.User.CreateManagedWithUsername(Input.Username, managedAccount, email: Input.Email);
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                else
                    return LocalRedirect(returnUrl);
            }

            // If we got this far, something failed, redisplay form printing errors.
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
