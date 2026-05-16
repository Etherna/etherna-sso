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
using Etherna.MongODM.Core.Attributes;
using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Etherna.SSOServer.Domain.Models
{
    public class ClientApp : EntityModelBase<string>
    {
        // Consts.
        public const int ClientIdRandomLength = 20;
        public const string ClientIdPrefix = "dev_";
        public const int ClientNameMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const string ClientIdValidChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string SecretValidChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const int SecretLength = 64;

        public static readonly string[] DeveloperAllowedIdentityScopes =
            [EthernaScopes.OpenId, EthernaScopes.Profile, EthernaScopes.EtherAccounts, EthernaScopes.Role];

        public static readonly string[] DeveloperAllowedApiScopes =
            [EthernaScopes.UserApiCredit, EthernaScopes.UserApiGateway, EthernaScopes.UserApiIndex, EthernaScopes.UserApiSso];

        public static readonly string[] DeveloperAllowedScopes =
            [..DeveloperAllowedIdentityScopes, ..DeveloperAllowedApiScopes];

        /// <summary>
        /// All scopes available to admin-created clients. Superset of <see cref="DeveloperAllowedScopes"/>,
        /// includes service-to-service interaction scopes not exposed to developer clients.
        /// </summary>
        public static readonly string[] AdminAllowedScopes =
            [..DeveloperAllowedScopes, EthernaScopes.EthernaCreditServiceInteract, EthernaScopes.EthernaSsoUserContactInfo];

        // Fields.
        private List<string> _allowedCorsOrigins = [];
        private List<string> _allowedGrantTypes = [];
        private List<string> _allowedScopes = [];
        private List<ClientSecret> _clientSecrets = [];
        private List<string> _postLogoutRedirectUris = [];
        private List<string> _redirectUris = [];

        // Constructors.
        /// <summary>
        /// Create a new client app with a specific client type template.
        /// </summary>
        public ClientApp(
            string clientName,
            string? description,
            ClientAppType clientType,
            UserBase owner,
            IEnumerable<string> allowedScopes,
            IEnumerable<string>? redirectUris = null,
            IEnumerable<string>? postLogoutRedirectUris = null,
            IEnumerable<string>? allowedCorsOrigins = null)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException($"'{nameof(clientName)}' cannot be null or whitespace.", nameof(clientName));
            if (clientName.Length > ClientNameMaxLength)
                throw new ArgumentException($"'{nameof(clientName)}' cannot be longer than {ClientNameMaxLength}.", nameof(clientName));
            if (description is not null && description.Length > DescriptionMaxLength)
                throw new ArgumentException($"'{nameof(description)}' cannot be longer than {DescriptionMaxLength}.", nameof(description));

            ClientId = GenerateClientId();
            ClientName = clientName;
            Description = description;
            ClientType = clientType;
            Enabled = true;
            Owner = owner;

            // Apply template defaults.
            switch (clientType)
            {
                case ClientAppType.WebApp:
                    _allowedGrantTypes = [GrantType.AuthorizationCode];
                    RequireClientSecret = true;
                    RequirePkce = false;
                    AllowOfflineAccess = true;
                    AlwaysIncludeUserClaimsInIdToken = true;
                    RequireConsent = false;
                    AccessTokenType = AccessTokenType.Jwt;
                    RefreshTokenUsage = TokenUsage.OneTimeOnly;
                    break;

                case ClientAppType.NativeApp:
                    _allowedGrantTypes = [GrantType.AuthorizationCode];
                    RequireClientSecret = false;
                    RequirePkce = true;
                    AllowOfflineAccess = true;
                    AlwaysIncludeUserClaimsInIdToken = true;
                    RequireConsent = false;
                    AccessTokenType = AccessTokenType.Jwt;
                    RefreshTokenUsage = TokenUsage.OneTimeOnly;
                    break;

                case ClientAppType.ClientCredential:
                    _allowedGrantTypes = [GrantType.ClientCredentials];
                    RequireClientSecret = true;
                    RequirePkce = false;
                    AllowOfflineAccess = false;
                    AlwaysIncludeUserClaimsInIdToken = false;
                    RequireConsent = false;
                    AccessTokenType = AccessTokenType.Jwt;
                    RefreshTokenUsage = TokenUsage.ReUse;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(clientType));
            }

            // Validate and set scopes.
            ArgumentNullException.ThrowIfNull(allowedScopes);
            foreach (var scope in allowedScopes)
            {
                if (!DeveloperAllowedScopes.Contains(scope))
                    throw new ArgumentException($"Scope '{scope}' is not allowed for developer clients.");
            }
            _allowedScopes = [..allowedScopes];

            // Set URIs.
            if (redirectUris is not null)
                _redirectUris = [..redirectUris];
            if (postLogoutRedirectUris is not null)
                _postLogoutRedirectUris = [..postLogoutRedirectUris];
            if (allowedCorsOrigins is not null)
                _allowedCorsOrigins = [..allowedCorsOrigins];
        }

        /// <summary>
        /// Create a custom client (admin-only).
        /// </summary>
        public ClientApp(
            string clientId,
            string clientName,
            string? description,
            UserBase owner,
            IEnumerable<string> allowedGrantTypes,
            bool requireClientSecret,
            bool requirePkce,
            bool allowOfflineAccess,
            bool alwaysIncludeUserClaimsInIdToken,
            bool requireConsent,
            AccessTokenType accessTokenType,
            TokenUsage refreshTokenUsage,
            IEnumerable<string> allowedScopes,
            IEnumerable<string>? redirectUris = null,
            IEnumerable<string>? postLogoutRedirectUris = null,
            IEnumerable<string>? allowedCorsOrigins = null)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException($"'{nameof(clientId)}' cannot be null or whitespace.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException($"'{nameof(clientName)}' cannot be null or whitespace.", nameof(clientName));
            ArgumentNullException.ThrowIfNull(owner);

            ClientId = clientId;
            ClientName = clientName;
            Description = description;
            ClientType = ClientAppType.Custom;
            Enabled = true;
            Owner = owner;

            _allowedGrantTypes = [..allowedGrantTypes];
            RequireClientSecret = requireClientSecret;
            RequirePkce = requirePkce;
            AllowOfflineAccess = allowOfflineAccess;
            AlwaysIncludeUserClaimsInIdToken = alwaysIncludeUserClaimsInIdToken;
            RequireConsent = requireConsent;
            AccessTokenType = accessTokenType;
            RefreshTokenUsage = refreshTokenUsage;

            // Validate and set scopes.
            ArgumentNullException.ThrowIfNull(allowedScopes);
            foreach (var scope in allowedScopes)
            {
                if (!AdminAllowedScopes.Contains(scope))
                    throw new ArgumentException($"Scope '{scope}' is not allowed for admin clients.");
            }
            _allowedScopes = [..allowedScopes];

            // Set URIs.
            if (redirectUris is not null)
                _redirectUris = [..redirectUris];
            if (postLogoutRedirectUris is not null)
                _postLogoutRedirectUris = [..postLogoutRedirectUris];
            if (allowedCorsOrigins is not null)
                _allowedCorsOrigins = [..allowedCorsOrigins];
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ClientApp() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual AccessTokenType AccessTokenType { get; set; }
        public virtual IEnumerable<string> AllowedCorsOrigins
        {
            get => _allowedCorsOrigins;
            set => _allowedCorsOrigins = [..value ?? []];
        }
        public virtual IEnumerable<string> AllowedGrantTypes
        {
            get => _allowedGrantTypes;
            set => _allowedGrantTypes = [..value ?? []];
        }
        public virtual IEnumerable<string> AllowedScopes
        {
            get => _allowedScopes;
            protected set => _allowedScopes = [..value ?? []];
        }
        public virtual bool AllowOfflineAccess { get; set; }
        public virtual bool AlwaysIncludeUserClaimsInIdToken { get; set; }
        public virtual string ClientId { get; protected set; }
        public virtual string ClientName { get; protected set; }
        public virtual ClientAppType ClientType { get; protected set; }
        public virtual IEnumerable<ClientSecret> ClientSecrets
        {
            get => _clientSecrets;
            protected set => _clientSecrets = [..value ?? []];
        }
        public virtual string? Description { get; protected set; }
        public virtual bool Enabled { get; set; }
        public virtual UserBase Owner { get; protected set; }
        public virtual IEnumerable<string> PostLogoutRedirectUris
        {
            get => _postLogoutRedirectUris;
            set => _postLogoutRedirectUris = [..value ?? []];
        }
        public virtual IEnumerable<string> RedirectUris
        {
            get => _redirectUris;
            set => _redirectUris = [..value ?? []];
        }
        public virtual TokenUsage RefreshTokenUsage { get; set; }
        public virtual bool RequireClientSecret { get; set; }
        public virtual bool RequireConsent { get; set; }
        public virtual bool RequirePkce { get; set; }

        // Methods.
        [PropertyAlterer(nameof(ClientSecrets))]
        public virtual void AddSecret(ClientSecret secret)
        {
            ArgumentNullException.ThrowIfNull(secret);
            _clientSecrets.Add(secret);
        }

        [PropertyAlterer(nameof(ClientSecrets))]
        public virtual bool RemoveSecret(string hashedValue)
        {
            var removed = _clientSecrets.RemoveAll(s => s.Value == hashedValue);
            return removed > 0;
        }

        [PropertyAlterer(nameof(ClientName))]
        [PropertyAlterer(nameof(Description))]
        public virtual void SetInfo(string clientName, string? description)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException($"'{nameof(clientName)}' cannot be null or whitespace.", nameof(clientName));
            if (clientName.Length > ClientNameMaxLength)
                throw new ArgumentException($"'{nameof(clientName)}' cannot be longer than {ClientNameMaxLength}.", nameof(clientName));
            if (description is not null && description.Length > DescriptionMaxLength)
                throw new ArgumentException($"'{nameof(description)}' cannot be longer than {DescriptionMaxLength}.", nameof(description));

            ClientName = clientName;
            Description = description;
        }

        [PropertyAlterer(nameof(AllowedScopes))]
        public virtual void SetAllowedScopes(IEnumerable<string> scopes)
        {
            ArgumentNullException.ThrowIfNull(scopes);

            if (ClientType != ClientAppType.Custom) // developer-created client
            {
                foreach (var scope in scopes)
                {
                    if (!DeveloperAllowedScopes.Contains(scope))
                        throw new ArgumentException($"Scope '{scope}' is not allowed for developer clients.");
                }
            }
            else // admin-created client
            {
                foreach (var scope in scopes)
                {
                    if (!AdminAllowedScopes.Contains(scope))
                        throw new ArgumentException($"Scope '{scope}' is not allowed for admin clients.");
                }
            }

            _allowedScopes = [..scopes];
        }

        // Static methods.
        public static string GenerateClientId() =>
            GenerateRandomString(ClientIdPrefix, ClientIdRandomLength, ClientIdValidChars);
        
        public static string GeneratePlainSecret() =>
            GenerateRandomString(null, SecretLength, SecretValidChars);
        
        public static string HashSecret(string plainSecret)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainSecret));
            return Convert.ToBase64String(bytes);
        }
        
        // Helpers.
        private static string GenerateRandomString(string? prefix, int randomLength, string validChars)
        {
            var codeBuilder = new StringBuilder(prefix);
            for (var i = 0; i < randomLength; i++)
                codeBuilder.Append(validChars[RandomNumberGenerator.GetInt32(validChars.Length)]);
            return codeBuilder.ToString();
        }
    }
}
