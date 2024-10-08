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

using Etherna.MongODM.Core.Extensions;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserClaimsModel : PageModel
    {
        // Models.
        public class ClaimDto
        {
            public ClaimDto(UserClaim claim)
            {
                ArgumentNullException.ThrowIfNull(claim, nameof(claim));

                Type = claim.Type;
                Value = claim.Value;
            }

            public string Type { get; }
            public string Value { get; }
        }
        public class InputModel
        {
            public string Type { get; set; } = "";
            public string Value { get; set; } = "";
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserClaimsModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string Id { get; private set; } = default!;
        public IEnumerable<ClaimDto> Claims { get; private set; } = Array.Empty<ClaimDto>();
        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public string Username { get; private set; } = default!;

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id, int? p)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            await InitializeAsync(id, p ?? 0);
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                await InitializeAsync(id, 0);
                return Page();
            }

            var user = await context.Users.FindOneAsync(id);
            user.AddClaim(new UserClaim(Input.Type, Input.Value));
            await context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }

        // Helpers.
        private async Task InitializeAsync(string id, int page)
        {
            CurrentPage = page;
            Id = id;

            var user = await context.Users.FindOneAsync(id);
            Claims = user.Claims.Paginate(c => (c.Type, c.Value), CurrentPage, PageSize)
                                .Select(c => new ClaimDto(c));
            MaxPage = (user.Claims.Count() - 1) / PageSize;
            Username = user.Username;
        }
    }
}
