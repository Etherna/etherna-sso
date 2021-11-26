//   Copyright 2021-present Etherna Sagl
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

using Etherna.Authentication;
using Etherna.RCL.Exceptions;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.SSOServer.Configs.IdentityServer
{
    public class IdServerConfig
    {
        // Consts.
        private static class ApiResourcesDef
        {
            public static readonly ApiResource EthernaCreditServiceInteract = new(
                "ethernaCreditServiceInteract",
                "Etherna Credit service interact")
            {
                Scopes = { ApiScopesDef.EthernaCreditServiceInteract.Name }
            };

            public static readonly ApiResource EthernaSsoServiceInteract = new(
                "ethernaSsoServiceInteract",
                "Etherna SSO service interact")
            {
                Scopes = { ApiScopesDef.EthernaSsoUserContactInfo.Name }
            };
        }
        private static class ApiScopesDef
        {
            public static readonly ApiScope EthernaCreditServiceInteract = new(
                "ethernaCredit_serviceInteract_api",
                "Etherna Credit service interact API");

            public static readonly ApiScope EthernaSsoUserContactInfo = new(
                "ethernaSso_userContactInfo_api",
                "Etherna SSO user contatct info API");
        }
        private static class IdResourcesDef
        {
            public static readonly IdentityResource EtherAccounts = new()
            {
                DisplayName = "Ether accounts",
                Name = "ether_accounts",
                UserClaims = new List<string>()
                {
                    ClaimTypes.EtherAddress,
                    ClaimTypes.EtherPreviousAddresses,
                    ClaimTypes.IsWeb3Account
                }
            };
            public static readonly IdentityResource Role = new()
            {
                DisplayName = "Role",
                Name = "role",
                UserClaims = new List<string>()
                {
                    ClaimTypes.Role
                }
            };
        }

        // Fields.
        private readonly string ethernaCredit_BaseUrl;
        private readonly string ethernaCredit_Webapp_ClientId;
        private readonly string ethernaCredit_Webapp_Secret;

        private readonly string ethernaDapp_BaseUrl;
        private readonly string ethernaDapp_ClientId;

        private readonly string[] ethernaGateway_BaseUrls;
        private readonly string ethernaGateway_Credit_ClientId;
        private readonly string ethernaGateway_Credit_Secret;
        private readonly string ethernaGateway_Webapp_ClientId;
        private readonly string ethernaGateway_Webapp_Secret;

        private readonly string ethernaIndex_BaseUrl;
        private readonly string ethernaIndex_Webapp_ClientId;
        private readonly string ethernaIndex_Webapp_Secret;

        // Constructor.
        public IdServerConfig(IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            ethernaCredit_BaseUrl = configuration["IdServer:Clients:EthernaCredit:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaCredit_Webapp_ClientId = configuration["IdServer:Clients:EthernaCredit:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaCredit_Webapp_Secret = configuration["IdServer:Clients:EthernaCredit:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

            ethernaDapp_BaseUrl = configuration["IdServer:Clients:EthernaDapp:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaDapp_ClientId = configuration["IdServer:Clients:EthernaDapp:ClientId"] ?? throw new ServiceConfigurationException();

            ethernaGateway_BaseUrls = configuration.GetSection("IdServer:Clients:EthernaGateway:BaseUrls").Get<string[]>() ?? throw new ServiceConfigurationException();
            ethernaGateway_Credit_ClientId = configuration["IdServer:Clients:EthernaGateway:Clients:Credit:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaGateway_Credit_Secret = configuration["IdServer:Clients:EthernaGateway:Clients:Credit:Secret"] ?? throw new ServiceConfigurationException();
            ethernaGateway_Webapp_ClientId = configuration["IdServer:Clients:EthernaGateway:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaGateway_Webapp_Secret = configuration["IdServer:Clients:EthernaGateway:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

            ethernaIndex_BaseUrl = configuration["IdServer:Clients:EthernaIndex:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaIndex_Webapp_ClientId = configuration["IdServer:Clients:EthernaIndex:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaIndex_Webapp_Secret = configuration["IdServer:Clients:EthernaIndex:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();
        }

        // Properties.
        public IEnumerable<ApiResource> ApiResources => new ApiResource[]
        {
            ApiResourcesDef.EthernaCreditServiceInteract,
            ApiResourcesDef.EthernaSsoServiceInteract
        };

        public IEnumerable<ApiScope> ApiScopes => new ApiScope[]
        {
            ApiScopesDef.EthernaCreditServiceInteract,
            ApiScopesDef.EthernaSsoUserContactInfo
        };

        public IEnumerable<Client> Clients => new Client[]
        {
            //credit (user login)
            new Client
            {
                ClientId = ethernaCredit_Webapp_ClientId,
                ClientName = "Etherna Credit",
                ClientSecrets = { new Secret(ethernaCredit_Webapp_Secret.Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                //where to redirect to after login
                RedirectUris = { $"{ethernaCredit_BaseUrl}/signin-oidc" },

                //where to redirect to after logout
                PostLogoutRedirectUris = { $"{ethernaCredit_BaseUrl}/signout-callback-oidc" },

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name
                },

                // Allow token refresh.
                AllowOfflineAccess = true
            },

            //dapp
            new Client
            {
                ClientId = ethernaDapp_ClientId,
                ClientName = "Etherna Dapp",
                RequireClientSecret = false,

                AllowedGrantTypes = GrantTypes.Code,
                    
                //where to redirect to after login
                RedirectUris = { $"{ethernaDapp_BaseUrl}/callback.html" },

                //where to redirect to after logout
                PostLogoutRedirectUris = { ethernaDapp_BaseUrl },

                AllowedCorsOrigins = { ethernaDapp_BaseUrl },

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name
                },

                // Allow token refresh.
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly //because client have not secret
            },

            //gateway (credit client)
            new Client
            {
                ClientId = ethernaGateway_Credit_ClientId,
                ClientName = "EthernaGateway Credit client",
                ClientSecrets = { new Secret(ethernaGateway_Credit_Secret.Sha256()) },

                //no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                //scopes that client has access to
                AllowedScopes =
                {
                    ApiScopesDef.EthernaCreditServiceInteract.Name
                }
            },
                
            //gateway (user login)
            new Client
            {
                ClientId = ethernaGateway_Webapp_ClientId,
                ClientName = "Etherna Gateway",
                ClientSecrets = { new Secret(ethernaGateway_Webapp_Secret.Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                //where to redirect to after login
                RedirectUris = ethernaGateway_BaseUrls.Select(url => $"{url}/signin-oidc").ToList(),

                //where to redirect to after logout
                PostLogoutRedirectUris = ethernaGateway_BaseUrls.Select(url => $"{url}/signout-callback-oidc").ToList(),

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name
                },

                // Allow token refresh.
                AllowOfflineAccess = true
            },
            
            //index (user login)
            new Client
            {
                ClientId = ethernaIndex_Webapp_ClientId,
                ClientName = "Etherna Index",
                ClientSecrets = { new Secret(ethernaIndex_Webapp_Secret.Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                //where to redirect to after login
                RedirectUris = { $"{ethernaIndex_BaseUrl}/signin-oidc" },

                //where to redirect to after logout
                PostLogoutRedirectUris = { $"{ethernaIndex_BaseUrl}/signout-callback-oidc" },

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name
                },

                // Allow token refresh.
                AllowOfflineAccess = true
            },
        };

        public IEnumerable<IdentityResource> IdResources => new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            IdResourcesDef.EtherAccounts,
            IdResourcesDef.Role
        };
    }
}
