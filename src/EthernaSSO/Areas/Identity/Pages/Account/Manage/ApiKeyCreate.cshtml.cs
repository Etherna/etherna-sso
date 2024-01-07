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

using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class ApiKeyCreateModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [DataType(DataType.Text)]
            [Display(Name = "Label")]
            [StringLength(maximumLength: ApiKey.LabelMaxLength, MinimumLength = 1)]
            public string Label { get; set; } = default!;

            [DataType(DataType.DateTime)]
            [Display(Name = "End of life (optional)")]
            public DateTime? EndOfLife { get; set; }
        }

        // Fields.
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public ApiKeyCreateModel(
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string? PrettyPrintedPlainKey { get; set; }
        public string? StatusMessage { get; set; }

        // Methods.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await userManager.GetUserAsync(User) ??
                throw new InvalidOperationException();

            // Get previously created keys by user.
            var prevKeys = await ssoDbContext.ApiKeys.QueryElementsAsync(elements =>
                elements.Where(k => k.Owner.Id == user.Id)
                        .ToListAsync());

            // Check conditions.
            //max limit
            if (prevKeys.Count >= ApiKey.MaxKeysPerUser)
            {
                StatusMessage = "Error: max number of keys has been reached";
                return Page();
            }

            //duplicate label
            if (prevKeys.Any(k => k.Label == Input.Label))
            {
                StatusMessage = $"Error: key with label {Input.Label} already exists";
                return Page();
            }

            //valid date
            if (Input.EndOfLife is not null &&
                Input.EndOfLife < DateTime.Now)
            {
                StatusMessage = $"Error: selected End of Life is already passed";
                return Page();
            }

            // Create and show new key.
            var plainKey = ApiKey.GetRandomPlainKey();
            PrettyPrintedPlainKey = ApiKey.GetPrettyPrintedPlainKey(plainKey, user);
            var apiKey = new ApiKey(plainKey, Input.Label, Input.EndOfLife, user);

            await ssoDbContext.ApiKeys.CreateAsync(apiKey);

            StatusMessage = $"Your new API Key has been created! Please note, this is the only time it will be displayed." +
                $" It's crucial that you store it in a secure location and do not share it." +
                $" If you lose it or if it becomes exposed, others may gain unauthorized access to the service as you." +
                $" If you believe your API Key has been compromised, immediately delete it and generate a new one." +
                $"\n\n{PrettyPrintedPlainKey}";
            return Page();
        }
    }
}
