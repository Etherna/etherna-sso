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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        // Fields.
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public IndexModel(
            UserManager<UserBase> userManager)
        {
            this.userManager = userManager;
        }

        // Properties.
        [Display(Name = "Ethereum address")]
        public string EtherAddress { get; set; } = default!;
        [Display(Name = "Ethereum previous addresses")]
        public IEnumerable<string> EtherPreviousAddresses { get; set; } = default!;
        public bool IsWeb3User { get; set; }
        public string Username { get; set; } = default!;

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            EtherAddress = user.EtherAddress;
            EtherPreviousAddresses = user.EtherPreviousAddresses;
            IsWeb3User = user is UserWeb3;
            Username = user.Username;

            return Page();
        }
    }
}
