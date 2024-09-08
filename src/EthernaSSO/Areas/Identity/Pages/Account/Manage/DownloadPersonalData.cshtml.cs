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

using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        // Fields.
        private readonly ILogger<DownloadPersonalDataModel> logger;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public DownloadPersonalDataModel(
            ILogger<DownloadPersonalDataModel> logger,
            UserManager<UserBase> userManager)
        {
            this.logger = logger;
            this.userManager = userManager;
        }

        // Methods.
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            logger.DownloadedPersonalData(userManager.GetUserId(User) ?? throw new InvalidOperationException());

            // Only include personal data for download
            var personalData = new Dictionary<string, object?>();
            var personalDataProps = user.GetType().GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
                personalData.Add(p.Name, p.GetValue(user));

            Response.Headers["Content-Disposition"] = "attachment; filename=PersonalData.json";
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }
    }
}
