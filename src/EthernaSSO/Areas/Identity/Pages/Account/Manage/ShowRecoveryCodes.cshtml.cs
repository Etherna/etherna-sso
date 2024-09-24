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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class ShowRecoveryCodesModel : PageModel
    {
        // Properties.
#pragma warning disable CA1819 // Properties should not return arrays
        [TempData]
        public string[] RecoveryCodes { get; set; } = default!; //array required by [TempData]
#pragma warning restore CA1819 // Properties should not return arrays

        [TempData]
        public string? StatusMessage { get; set; }

        // Methods.
        public IActionResult OnGet()
        {
            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }

            return Page();
        }
    }
}
