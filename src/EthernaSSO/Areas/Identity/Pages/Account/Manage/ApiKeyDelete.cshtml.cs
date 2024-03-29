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
