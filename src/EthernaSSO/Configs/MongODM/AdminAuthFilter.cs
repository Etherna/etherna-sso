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

using Etherna.Authentication;
using Etherna.MongODM.AspNetCore.UI.Auth.Filters;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.MongODM
{
    public class AdminAuthFilter : IDashboardAuthFilter
    {
        public async Task<bool> AuthorizeAsync(HttpContext? context)
        {
            if (context?.User.Identity?.IsAuthenticated != true)
                return false;

            // Verify role on claims, without db reads: keeps the dashboard reachable while a migration locks the db context.
            var ethernaOidcClient = context.RequestServices.GetRequiredService<IEthernaOpenIdConnectClient>();
            var roles = await ethernaOidcClient.TryGetRolesAsync();

            return roles?.Contains(Role.NormalizeName(Role.AdministratorName)) == true;
        }
    }
}
