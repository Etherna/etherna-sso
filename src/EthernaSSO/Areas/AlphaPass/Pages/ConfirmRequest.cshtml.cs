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

using Etherna.ACR.Helpers;
using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.AlphaPass.Pages
{
    public class ConfirmRequestModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext dbContext;

        // Constructor.
        public ConfirmRequestModel(
            ISsoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Properties.
        [TempData]
        public string? StatusMessage { get; set; }

        // Get.

        public async Task OnGetAsync(string email, string secret)
        {
            var request = await dbContext.AlphaPassRequests.TryFindOneAsync(r => r.NormalizedEmail == EmailHelper.NormalizeEmail(email));

            // Verify provided data.
            if (request is null ||
                request.Secret != secret)
            {
                StatusMessage = "Error: provided data is not correct";
                return;
            }

            // Confirm email.
            request.ConfirmEmail(secret);
            await dbContext.SaveChangesAsync();

            StatusMessage = "Thank you, your email has been verified! You will receive an Etherna Alpha Pass when available.";
        }
    }
}
