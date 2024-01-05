// Copyright 2021-present Etherna Sa
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
