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
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using Etherna.SSOServer.Services.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage.Developer
{
    public class ClientDeleteModel(
        ILogger<ClientDeleteModel> logger,
        ISsoDbContext ssoDbContext,
        UserManager<UserBase> userManager)
        : PageModel
    {
        // Properties.
        public string Id { get; private set; } = null!;

        [Display(Name = "Client ID")]
        public string ClientId { get; private set; } = null!;

        [Display(Name = "Client Name")]
        public string ClientName { get; private set; } = null!;

        // Methods.
        public async Task<IActionResult> OnGetAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var clientApp = await ssoDbContext.ClientApps.FindOneAsync(id);

            var userId = userManager.GetUserId(User);
            if (clientApp.ClientType == ClientAppType.Custom || clientApp.Owner.Id != userId)
                return Forbid();

            Id = id;
            ClientId = clientApp.ClientId;
            ClientName = clientApp.ClientName;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var clientApp = await ssoDbContext.ClientApps.FindOneAsync(id);

            var userId = userManager.GetUserId(User);
            if (clientApp.ClientType == ClientAppType.Custom || clientApp.Owner.Id != userId)
                return Forbid();

            var clientId = clientApp.ClientId;
            await ssoDbContext.ClientApps.DeleteAsync(id);
            logger.ClientAppDeleted(userId, clientId);

            return RedirectToPage("Index");
        }
    }
}
