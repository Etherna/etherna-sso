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
        [TempData]
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
