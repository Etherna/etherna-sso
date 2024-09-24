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
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class ApiKeyDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructor.
        public ApiKeyDeleteModel(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Properties.
        public string Id { get; private set; } = default!;

        [Display(Name = "End of life")]
        public DateTime? EndOfLife { get; private set; }

        public string Label { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            var apiKey = await ssoDbContext.ApiKeys.FindOneAsync(id);

            Id = id;
            EndOfLife = apiKey.EndOfLife;
            Label = apiKey.Label;
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await ssoDbContext.ApiKeys.DeleteAsync(id);
            return RedirectToPage("ApiKeys");
        }
    }
}
