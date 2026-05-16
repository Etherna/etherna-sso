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
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class ClientSecretDeleteModel(ISsoDbContext context) : PageModel
    {
        // Properties.
        public string Id { get; private set; } = null!;

        [Display(Name = "Client Name")]
        public string ClientName { get; private set; } = null!;

        [Display(Name = "Description")]
        public string? Description { get; private set; }

        [Display(Name = "Expiration")]
        public DateTime? Expiration { get; private set; }

        public string SecretHash { get; private set; } = null!;

        // Methods.
        public async Task OnGetAsync(string id, string secretHash)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(secretHash);

            Id = id;
            SecretHash = secretHash;

            var clientApp = await context.ClientApps.FindOneAsync(id);
            ClientName = clientApp.ClientName;

            var secret = clientApp.ClientSecrets.FirstOrDefault(s => s.Value == secretHash);
            if (secret is not null)
            {
                Description = secret.Description;
                Expiration = secret.Expiration;
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id, string secretHash)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(secretHash);

            var clientApp = await context.ClientApps.FindOneAsync(id);
            clientApp.RemoveSecret(secretHash);
            await context.SaveChangesAsync();

            return RedirectToPage("ClientSecrets", new { id });
        }
    }
}
