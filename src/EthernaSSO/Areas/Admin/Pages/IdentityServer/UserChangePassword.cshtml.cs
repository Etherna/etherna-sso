using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserChangePasswordModel : PageModel
    {
        // Model.
        public class InputModel
        {
            [Required]
            public string Password { get; set; } = default!;

            [Required]
            [Compare(nameof(Password))]
            [Display(Name = "Confirm password")]
            public string ConfirmPassword { get; set; } = default!;
        }

        // Fields.
        private readonly ISsoDbContext context;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public UserChangePasswordModel(
            ISsoDbContext context,
            UserManager<UserBase> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        // Properties.
        public string Id { get; private set; } = default!;
        public string Username { get; private set; } = default!;

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            Id = id;
            var user = await context.Users.FindOneAsync(id);
            Username = user.Username;
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            Id = id;
            var user = await context.Users.FindOneAsync(id);
            Username = user.Username;

            if (!ModelState.IsValid)
                return Page();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            return RedirectToPage("User", new { id });
        }
    }
}
