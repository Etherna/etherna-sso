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

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.SystemStore
{
    /// <summary>
    /// CORS policy service that checks both in-memory and database-stored clients.
    /// </summary>
    internal sealed class CompositeCorsPolicyService(
        ISsoDbContext ssoDbContext,
        Client[] inMemoryClients)
        : ICorsPolicyService
    {
        // Methods.
        public async Task<bool> IsOriginAllowedAsync(string origin, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(origin))
                return false;

            // Check in-memory clients first (fast).
            var isAllowedInMemory = inMemoryClients.Any(c =>
                c.AllowedCorsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase));

            if (isAllowedInMemory)
                return true;

            // Check database clients.
            var isAllowedInDb = await ssoDbContext.ClientApps.QueryElementsAsync(elements =>
                elements.AnyAsync(c =>
                    c.Enabled &&
                    c.AllowedCorsOrigins.Contains(origin), cancellationToken: cancellationToken));

            return isAllowedInDb;
        }
    }
}
