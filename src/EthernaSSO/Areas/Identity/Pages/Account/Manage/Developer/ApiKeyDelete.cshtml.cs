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

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage.Developer
{
    public class ApiKeyDeleteModel(ISsoDbContext ssoDbContext) : PageModel
    {
        // Properties.
        public string Id { get; private set; } = null!;

        [Display(Name = "End of life (UTC)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? EndOfLife { get; private set; }

        public string Label { get; private set; } = null!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

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
