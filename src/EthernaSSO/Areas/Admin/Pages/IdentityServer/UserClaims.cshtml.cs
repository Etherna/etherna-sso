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
