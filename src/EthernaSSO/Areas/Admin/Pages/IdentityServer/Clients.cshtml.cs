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

using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Configs.IdentityServer;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class ClientsModel(
        ISsoDbContext ssoDbContext,
        IdServerConfig idServerConfig)
        : PageModel
    {
        // Models.
        public class ClientDto
        {
            public ClientDto(ClientApp clientApp)
            {
                ArgumentNullException.ThrowIfNull(clientApp);

                Id = clientApp.Id;
                ClientId = clientApp.ClientId;
                ClientName = clientApp.ClientName;
                ClientType = clientApp.ClientType;
                GrantTypes = string.Join(", ", clientApp.AllowedGrantTypes);
                Enabled = clientApp.Enabled;
                IsInMemory = false;
                OwnerUsername = clientApp.Owner.Username;
            }

            public ClientDto(Duende.IdentityServer.Models.Client client)
            {
                ArgumentNullException.ThrowIfNull(client);

                Id = null;
                ClientId = client.ClientId;
                ClientName = client.ClientName;
                ClientType = null;
                GrantTypes = string.Join(", ", client.AllowedGrantTypes);
                Enabled = client.Enabled;
                IsInMemory = true;
                OwnerUsername = null;
            }

            public string? Id { get; }
            public string ClientId { get; }
            public string? ClientName { get; }
            public ClientAppType? ClientType { get; }
            public string? GrantTypes { get; }
            public bool Enabled { get; }
            public bool IsInMemory { get; }
            public string? OwnerUsername { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Properties.
        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
        public string? Query { get; private set; }
        public List<ClientDto> Clients { get; } = new();

        // Methods.
        public async Task OnGetAsync(int? p, string? q)
        {
            CurrentPage = p ?? 0;
            Query = q ?? "";

            // Get in-memory clients.
            var inMemoryClients = idServerConfig.Clients
                .Where(c => string.IsNullOrEmpty(Query) ||
                    (c.ClientId?.Contains(Query, StringComparison.OrdinalIgnoreCase) == true) ||
                    (c.ClientName?.Contains(Query, StringComparison.OrdinalIgnoreCase) == true))
                .Select(c => new ClientDto(c))
                .ToList();

            // Get DB clients.
            var dbClients = await ssoDbContext.ClientApps.QueryElementsAsync(elements =>
                elements.Where(c => string.IsNullOrEmpty(Query) ||
                    c.ClientId.Contains(Query) ||
                    c.ClientName.Contains(Query))
                    .ToListAsync());

            var dbClientDtos = dbClients.Select(c => new ClientDto(c)).ToList();

            // Merge and paginate.
            var allClients = dbClientDtos
                .Concat(inMemoryClients)
                .OrderBy(c => c.ClientId)
                .ToList();

            MaxPage = allClients.Count > 0 ? (allClients.Count - 1) / PageSize : 0;

            Clients.AddRange(allClients.Skip(CurrentPage * PageSize).Take(PageSize));
        }
    }
}
