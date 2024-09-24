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
