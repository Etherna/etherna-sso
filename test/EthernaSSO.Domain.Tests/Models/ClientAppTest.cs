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
using Etherna.Authentication;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Etherna.SSOServer.Domain.Models
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class ClientAppTest
    {
        // Helpers.
        private static UserWeb2 CreateOwner() => UserWeb2Builder.Build();

        private static ClientApp CreateWebAppClient() =>
            new(
                clientName: "Test Client",
                description: "A test client",
                clientType: ClientAppType.WebApp,
                owner: CreateOwner(),
                allowedScopes: [EthernaScopes.OpenId, EthernaScopes.Profile]);

        private static ClientApp CreateCustomClient() =>
            new(
                clientId: "custom_client_id",
                clientName: "Custom Client",
                description: null,
                owner: CreateOwner(),
                allowedGrantTypes: [GrantType.ClientCredentials],
                requireClientSecret: true,
                requirePkce: false,
                allowOfflineAccess: false,
                alwaysIncludeUserClaimsInIdToken: false,
                requireConsent: false,
                accessTokenType: AccessTokenType.Jwt,
                refreshTokenUsage: TokenUsage.ReUse,
                allowedScopes: [EthernaScopes.EthernaCreditServiceInteract]);

        // Tests.
        [Fact]
        public void Constructor_WithWebAppType_AppliesTemplateDefaults()
        {
            // Arrange & Action.
            var client = CreateWebAppClient();

            // Assert.
            Assert.Equal(ClientAppType.WebApp, client.ClientType);
            Assert.True(client.Enabled);
            Assert.True(client.RequireClientSecret);
            Assert.False(client.RequirePkce);
            Assert.True(client.AllowOfflineAccess);
            Assert.Contains(GrantType.AuthorizationCode, client.AllowedGrantTypes);
            Assert.StartsWith(ClientApp.ClientIdPrefix, client.ClientId, StringComparison.Ordinal);
        }

        [Fact]
        public void Constructor_WithNativeAppType_AppliesTemplateDefaults()
        {
            // Arrange & Action.
            var client = new ClientApp(
                clientName: "Native",
                description: null,
                clientType: ClientAppType.NativeApp,
                owner: CreateOwner(),
                allowedScopes: [EthernaScopes.OpenId]);

            // Assert.
            Assert.False(client.RequireClientSecret);
            Assert.True(client.RequirePkce);
            Assert.True(client.AllowOfflineAccess);
            Assert.Contains(GrantType.AuthorizationCode, client.AllowedGrantTypes);
        }

        [Fact]
        public void Constructor_WithClientCredentialType_AppliesTemplateDefaults()
        {
            // Arrange & Action.
            var client = new ClientApp(
                clientName: "Service",
                description: null,
                clientType: ClientAppType.ClientCredential,
                owner: CreateOwner(),
                allowedScopes: [EthernaScopes.UserApiSso]);

            // Assert.
            Assert.True(client.RequireClientSecret);
            Assert.False(client.RequirePkce);
            Assert.False(client.AllowOfflineAccess);
            Assert.Contains(GrantType.ClientCredentials, client.AllowedGrantTypes);
        }

        [Fact]
        public void Constructor_WithDisallowedScopeForDeveloper_ThrowsArgumentException()
        {
            // Action & Assert.
            Assert.Throws<ArgumentException>(() => new ClientApp(
                clientName: "Test",
                description: null,
                clientType: ClientAppType.WebApp,
                owner: CreateOwner(),
                allowedScopes: [EthernaScopes.EthernaCreditServiceInteract])); //admin-only scope
        }

        [Fact]
        public void Constructor_WithEmptyClientName_ThrowsArgumentException()
        {
            // Action & Assert.
            Assert.Throws<ArgumentException>(() => new ClientApp(
                clientName: "   ",
                description: null,
                clientType: ClientAppType.WebApp,
                owner: CreateOwner(),
                allowedScopes: [EthernaScopes.OpenId]));
        }

        [Fact]
        public void Constructor_WithTooLongClientName_ThrowsArgumentException()
        {
            // Arrange.
            var longName = new string('a', ClientApp.ClientNameMaxLength + 1);

            // Action & Assert.
            Assert.Throws<ArgumentException>(() => new ClientApp(
                clientName: longName,
                description: null,
                clientType: ClientAppType.WebApp,
                owner: CreateOwner(),
                allowedScopes: [EthernaScopes.OpenId]));
        }

        [Fact]
        public void Constructor_WithTooLongDescription_ThrowsArgumentException()
        {
            // Arrange.
            var longDescription = new string('a', ClientApp.DescriptionMaxLength + 1);

            // Action & Assert.
            Assert.Throws<ArgumentException>(() => new ClientApp(
                clientName: "Test",
                description: longDescription,
                clientType: ClientAppType.WebApp,
                owner: CreateOwner(),
                allowedScopes: [EthernaScopes.OpenId]));
        }

        [Fact]
        public void CustomConstructor_WithAdminOnlyScope_Succeeds()
        {
            // Arrange & Action.
            var client = CreateCustomClient();

            // Assert.
            Assert.Equal(ClientAppType.Custom, client.ClientType);
            Assert.Contains(EthernaScopes.EthernaCreditServiceInteract, client.AllowedScopes);
        }

        [Fact]
        public void CustomConstructor_WithUnknownScope_ThrowsArgumentException()
        {
            // Action & Assert.
            Assert.Throws<ArgumentException>(() => new ClientApp(
                clientId: "id",
                clientName: "Name",
                description: null,
                owner: CreateOwner(),
                allowedGrantTypes: [GrantType.ClientCredentials],
                requireClientSecret: true,
                requirePkce: false,
                allowOfflineAccess: false,
                alwaysIncludeUserClaimsInIdToken: false,
                requireConsent: false,
                accessTokenType: AccessTokenType.Jwt,
                refreshTokenUsage: TokenUsage.ReUse,
                allowedScopes: ["not_a_valid_scope"]));
        }

        [Fact]
        public void CustomConstructor_WithEmptyClientId_ThrowsArgumentException()
        {
            // Action & Assert.
            Assert.Throws<ArgumentException>(() => new ClientApp(
                clientId: " ",
                clientName: "Name",
                description: null,
                owner: CreateOwner(),
                allowedGrantTypes: [GrantType.ClientCredentials],
                requireClientSecret: true,
                requirePkce: false,
                allowOfflineAccess: false,
                alwaysIncludeUserClaimsInIdToken: false,
                requireConsent: false,
                accessTokenType: AccessTokenType.Jwt,
                refreshTokenUsage: TokenUsage.ReUse,
                allowedScopes: [EthernaScopes.OpenId]));
        }

        [Fact]
        public void AddSecret_WithValidSecret_AppendsToClientSecrets()
        {
            // Arrange.
            var client = CreateWebAppClient();
            var secret = new ClientSecret("hashedValue", "primary", expiration: null);

            // Action.
            client.AddSecret(secret);

            // Assert.
            Assert.Contains(client.ClientSecrets, s => s.Value == "hashedValue");
        }

        [Fact]
        public void AddSecret_WithNull_ThrowsArgumentNullException()
        {
            // Arrange.
            var client = CreateWebAppClient();

            // Action & Assert.
            Assert.Throws<ArgumentNullException>(() => client.AddSecret(null!));
        }

        [Fact]
        public void AddSecret_CalledMultipleTimes_AllowsDuplicateValues()
        {
            // Arrange.
            var client = CreateWebAppClient();

            // Action.
            client.AddSecret(new ClientSecret("hashed", null, null));
            client.AddSecret(new ClientSecret("hashed", null, null));

            // Assert.
            Assert.Equal(2, client.ClientSecrets.Count());
        }

        [Fact]
        public void RemoveSecret_WithExistingHashedValue_RemovesAndReturnsTrue()
        {
            // Arrange.
            var client = CreateWebAppClient();
            client.AddSecret(new ClientSecret("hashedValue", null, null));

            // Action.
            var result = client.RemoveSecret("hashedValue");

            // Assert.
            Assert.True(result);
            Assert.DoesNotContain(client.ClientSecrets, s => s.Value == "hashedValue");
        }

        [Fact]
        public void RemoveSecret_WithNonExistingHashedValue_ReturnsFalse()
        {
            // Arrange.
            var client = CreateWebAppClient();
            client.AddSecret(new ClientSecret("other", null, null));

            // Action.
            var result = client.RemoveSecret("missing");

            // Assert.
            Assert.False(result);
            Assert.Single(client.ClientSecrets);
        }

        [Fact]
        public void SetInfo_WithValidValues_UpdatesNameAndDescription()
        {
            // Arrange.
            var client = CreateWebAppClient();

            // Action.
            client.SetInfo("New Name", "New description");

            // Assert.
            Assert.Equal("New Name", client.ClientName);
            Assert.Equal("New description", client.Description);
        }

        [Fact]
        public void SetInfo_WithNullDescription_ClearsDescription()
        {
            // Arrange.
            var client = CreateWebAppClient();

            // Action.
            client.SetInfo("New Name", null);

            // Assert.
            Assert.Equal("New Name", client.ClientName);
            Assert.Null(client.Description);
        }

        [Fact]
        public void SetInfo_WithEmptyClientName_ThrowsArgumentException()
        {
            // Arrange.
            var client = CreateWebAppClient();

            // Action & Assert.
            Assert.Throws<ArgumentException>(() => client.SetInfo(" ", "desc"));
        }

        [Fact]
        public void SetInfo_WithTooLongClientName_ThrowsArgumentException()
        {
            // Arrange.
            var client = CreateWebAppClient();
            var longName = new string('a', ClientApp.ClientNameMaxLength + 1);

            // Action & Assert.
            Assert.Throws<ArgumentException>(() => client.SetInfo(longName, "desc"));
        }

        [Fact]
        public void SetInfo_WithTooLongDescription_ThrowsArgumentException()
        {
            // Arrange.
            var client = CreateWebAppClient();
            var longDescription = new string('a', ClientApp.DescriptionMaxLength + 1);

            // Action & Assert.
            Assert.Throws<ArgumentException>(() => client.SetInfo("Name", longDescription));
        }

        [Fact]
        public void SetAllowedScopes_OnDeveloperClient_WithAllowedScopes_UpdatesScopes()
        {
            // Arrange.
            var client = CreateWebAppClient();

            // Action.
            client.SetAllowedScopes([EthernaScopes.OpenId, EthernaScopes.Role]);

            // Assert.
            Assert.Equal(2, client.AllowedScopes.Count());
            Assert.Contains(EthernaScopes.OpenId, client.AllowedScopes);
            Assert.Contains(EthernaScopes.Role, client.AllowedScopes);
        }

        [Fact]
        public void SetAllowedScopes_OnDeveloperClient_WithAdminOnlyScope_ThrowsArgumentException()
        {
            // Arrange.
            var client = CreateWebAppClient();

            // Action & Assert.
            Assert.Throws<ArgumentException>(() =>
                client.SetAllowedScopes([EthernaScopes.EthernaCreditServiceInteract]));
        }

        [Fact]
        public void SetAllowedScopes_OnCustomClient_WithAdminOnlyScope_Succeeds()
        {
            // Arrange.
            var client = CreateCustomClient();

            // Action.
            client.SetAllowedScopes([EthernaScopes.EthernaSsoUserContactInfo]);

            // Assert.
            Assert.Single(client.AllowedScopes);
            Assert.Contains(EthernaScopes.EthernaSsoUserContactInfo, client.AllowedScopes);
        }

        [Fact]
        public void SetAllowedScopes_OnCustomClient_WithUnknownScope_ThrowsArgumentException()
        {
            // Arrange.
            var client = CreateCustomClient();

            // Action & Assert.
            Assert.Throws<ArgumentException>(() =>
                client.SetAllowedScopes(["not_a_valid_scope"]));
        }

        [Fact]
        public void SetAllowedScopes_WithNull_ThrowsArgumentNullException()
        {
            // Arrange.
            var client = CreateWebAppClient();

            // Action & Assert.
            Assert.Throws<ArgumentNullException>(() => client.SetAllowedScopes(null!));
        }
    }
}
