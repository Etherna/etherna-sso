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
        public async Task<IActionResult> OnGetAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            Id = id;
            var user = await context.Users.FindOneAsync(id);
            Username = user.Username;

            if (user is not UserWeb2)
                return RedirectToPage("User", new { id });

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            Id = id;
            var user = await context.Users.FindOneAsync(id);
            Username = user.Username;

            if (user is not UserWeb2)
                return RedirectToPage("User", new { id });
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
