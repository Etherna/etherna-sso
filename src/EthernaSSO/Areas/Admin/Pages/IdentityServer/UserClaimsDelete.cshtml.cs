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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserClaimsDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserClaimsDeleteModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string ClaimType { get; private set; } = default!;
        public string ClaimValue { get; private set; } = default!;
        public string UserId { get; private set; } = default!;
        public string Username { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string claimType, string claimValue, string userId)
        {
            ArgumentNullException.ThrowIfNull(claimType, nameof(claimType));
            ArgumentNullException.ThrowIfNull(claimValue, nameof(claimValue));
            ArgumentNullException.ThrowIfNull(userId, nameof(userId));

            var user = await context.Users.FindOneAsync(userId);

            ClaimType = claimType;
            ClaimValue = claimValue;
            UserId = userId;
            Username = user.Username;
        }

        public async Task<IActionResult> OnPostAsync(string claimType, string claimValue, string userId)
        {
            ArgumentNullException.ThrowIfNull(claimType, nameof(claimType));
            ArgumentNullException.ThrowIfNull(claimValue, nameof(claimValue));
            ArgumentNullException.ThrowIfNull(userId, nameof(userId));

            var user = await context.Users.FindOneAsync(userId);
            user.RemoveClaim(claimType, claimValue);
            await context.SaveChangesAsync();

            return RedirectToPage("UserClaims", new { id = userId });
        }
    }
}
