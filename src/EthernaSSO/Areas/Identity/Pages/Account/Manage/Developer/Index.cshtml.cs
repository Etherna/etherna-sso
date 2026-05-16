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

using Etherna.BeeNet.Models;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage.Developer
{
    public class IndexModel(
        ISsoDbContext ssoDbContext,
        UserManager<UserBase> userManager)
        : PageModel
    {
        // Properties.
        public List<ClientApp> Clients { get; } = [];
        public bool IsAdmin { get; set; }
        public int MaxAllowedClients { get; set; }
        public bool CanCreateClients => IsAdmin || Clients.Count < MaxAllowedClients;
        public bool LimitReached => !IsAdmin && Clients.Count >= MaxAllowedClients && MaxAllowedClients > 0;
        public EthAddress UserEtherAddress { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            IsAdmin = await userManager.IsInRoleAsync(user, Role.AdministratorName);
            MaxAllowedClients = user.MaxAllowedClients;
            UserEtherAddress = user.EtherAddress;

            var userId = userManager.GetUserId(User);
            var clients = await ssoDbContext.ClientApps.QueryElementsAsync(elements =>
                elements.Where(c => c.Owner.Id == userId && c.ClientType != ClientAppType.Custom)
                        .OrderBy(c => c.CreationDateTime)
                        .ToListAsync());
            Clients.AddRange(clients);

            return Page();
        }
    }
}
