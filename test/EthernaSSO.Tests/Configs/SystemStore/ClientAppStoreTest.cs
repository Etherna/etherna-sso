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
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Etherna.SwarmSdk.Models;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Etherna.SSOServer.Configs.SystemStore
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class ClientAppStoreTest
    {
        // Consts.
        private const string OwnerAddress = "0x123f681646d4a755815f9cb19e1acc8565a0c2ac";

        // Helpers.
        private static UserWeb3 CreateOwner() =>
            new UserWeb3("owner", invitedBy: null, invitedByAdmin: false, new UserSharedInfo(new EthAddress(OwnerAddress)));

        // Tests.
        [Fact]
        public void ToIdentityServerClient_ClientCredentialClient_EmbedsOwnerEtherAddressAsClientClaim()
        {
            // Arrange.
            var owner = CreateOwner();
            var clientApp = new ClientApp(
                clientName: "M2M Client",
                description: null,
                clientType: ClientAppType.ClientCredential,
                owner: owner,
                allowedScopes: [EthernaScopes.UserApiGateway]);

            // Act.
            var client = ClientAppStore.ToIdentityServerClient(clientApp);

            // Assert.
            // The owner's ether address must be sent as an unprefixed client claim, so downstream
            // services (e.g. the Gateway) bill the application's consumption to its owner.
            Assert.True(client.AlwaysSendClientClaims);
            Assert.Equal("", client.ClientClaimsPrefix);
            var claim = Assert.Single(client.Claims);
            Assert.Equal(EthernaClaimTypes.EtherAddress, claim.Type);
            Assert.Equal(owner.EtherAddress.ToString(), claim.Value);
        }

        [Fact]
        public void ToIdentityServerClient_InteractiveClient_DoesNotEmbedOwnerEtherAddressClaim()
        {
            // Arrange.
            // Interactive flows must carry the logged-in user's ether address, not the owner's.
            var clientApp = new ClientApp(
                clientName: "Web Client",
                description: null,
                clientType: ClientAppType.WebApp,
                owner: CreateOwner(),
                allowedScopes: [EthernaScopes.UserApiGateway]);

            // Act.
            var client = ClientAppStore.ToIdentityServerClient(clientApp);

            // Assert.
            Assert.Empty(client.Claims);
            Assert.False(client.AlwaysSendClientClaims);
        }
    }
}
