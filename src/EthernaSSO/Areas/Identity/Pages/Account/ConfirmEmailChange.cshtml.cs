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

using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailChangeModel : PageModel
    {
        // Fields.
        private readonly UserManager<UserBase> _userManager;
        private readonly SignInManager<UserBase> _signInManager;

        // Constructor.
        public ConfirmEmailChangeModel(UserManager<UserBase> userManager, SignInManager<UserBase> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Properties.
        public string? StatusMessage { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            // Validate.
            if (userId == null || email == null || code == null)
                return RedirectToPage("/Index");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userId}'.");

            // Confirm.
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Error changing email.");
                foreach (var error in result.Errors)
                    stringBuilder.Append(" " + error.Description ?? error.Code);

                StatusMessage = stringBuilder.ToString();
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Thank you for confirming your email change.";
            return Page();
        }
    }
}
