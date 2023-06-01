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

using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class ApiKeysModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public ApiKeysModel(
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Properties.
        public List<ApiKey> ApiKeys { get; } = new();

        // Methods.
        public async Task<IActionResult> OnGet()
        {
            var userId = userManager.GetUserId(User);
            var apiKeys = await ssoDbContext.ApiKeys.QueryElementsAsync(elements =>
                elements.Where(k => k.Owner.Id == userId)
                        .OrderBy(k => k.CreationDateTime)
                        .ToListAsync());
            ApiKeys.AddRange(apiKeys);

            return Page();
        }
    }
}
