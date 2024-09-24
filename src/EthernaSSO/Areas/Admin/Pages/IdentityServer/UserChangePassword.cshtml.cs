// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

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
