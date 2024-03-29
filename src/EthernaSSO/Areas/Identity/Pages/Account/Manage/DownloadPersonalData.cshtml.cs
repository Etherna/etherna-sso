﻿// Copyright 2021-present Etherna Sa
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
