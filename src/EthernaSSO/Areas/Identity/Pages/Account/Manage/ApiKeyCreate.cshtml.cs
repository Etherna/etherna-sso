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
