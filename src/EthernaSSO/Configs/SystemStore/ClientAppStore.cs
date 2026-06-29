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
using Duende.IdentityServer.Stores;
using Etherna.Authentication;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.SystemStore
{
    internal sealed class ClientAppStore(
        ISsoDbContext ssoDbContext,
        Client[] inMemoryClients)
        : IClientStore
    {
        // Methods.
        public async Task<Client?> FindClientByIdAsync(string clientId, CancellationToken cancellationToken = default)
        {
            // Check DB first.
            var dbClient = await ssoDbContext.ClientApps.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken: cancellationToken));

            if (dbClient is not null)
                return ToIdentityServerClient(dbClient);

            // Fall back to in-memory.
            return inMemoryClients.FirstOrDefault(c => c.ClientId == clientId);
        }

        public async IAsyncEnumerable<Client> GetAllClientsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // DB clients take precedence over in-memory ones sharing the same id.
            var dbClients = await ssoDbContext.ClientApps.QueryElementsAsync(elements =>
                elements.ToListAsync(cancellationToken: cancellationToken));
            var dbClientIds = dbClients.Select(c => c.ClientId).ToHashSet();

            foreach (var dbClient in dbClients)
                yield return ToIdentityServerClient(dbClient);

            foreach (var inMemoryClient in inMemoryClients)
                if (!dbClientIds.Contains(inMemoryClient.ClientId))
                    yield return inMemoryClient;
        }

        // Helpers.
        internal static Client ToIdentityServerClient(ClientApp clientApp)
        {
            var client = new Client
            {
                ClientId = clientApp.ClientId,
                ClientName = clientApp.ClientName,
                Description = clientApp.Description,
                Enabled = clientApp.Enabled,
                RequireClientSecret = clientApp.RequireClientSecret,
                RequirePkce = clientApp.RequirePkce,
                AllowOfflineAccess = clientApp.AllowOfflineAccess,
                AlwaysIncludeUserClaimsInIdToken = clientApp.AlwaysIncludeUserClaimsInIdToken,
                RequireConsent = clientApp.RequireConsent,
                AccessTokenType = clientApp.AccessTokenType,
                RefreshTokenUsage = clientApp.RefreshTokenUsage,
                AllowedGrantTypes = clientApp.AllowedGrantTypes.ToList(),
                AllowedScopes = clientApp.AllowedScopes.ToList(),
                RedirectUris = clientApp.RedirectUris.ToList(),
                PostLogoutRedirectUris = clientApp.PostLogoutRedirectUris.ToList(),
                AllowedCorsOrigins = clientApp.AllowedCorsOrigins.ToList(),
                ClientSecrets = clientApp.ClientSecrets
                    .Where(s => !s.IsExpired)
                    .Select(s => new Secret(s.Value, s.Description ?? "", s.Expiration))
                    .ToList()
            };

            // Machine-to-machine clients have no interactive user, so downstream services
            // (e.g. the Gateway) can't resolve an ether address to bill from user claims.
            // Embed the owner's ether address as a client claim so the application's consumption
            // is attributed to its owner. Interactive flows must keep the logged-in user's address.
            if (clientApp.ClientType == ClientAppType.ClientCredential)
            {
                client.Claims.Add(new ClientClaim(EthernaClaimTypes.EtherAddress, clientApp.Owner.EtherAddress.ToString()));
                client.AlwaysSendClientClaims = true;
                client.ClientClaimsPrefix = ""; // emit as "ether_address", not "client_ether_address"
            }

            return client;
        }
    }
}
