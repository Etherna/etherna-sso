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
