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

using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Exceptions;
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
        // Fields.
        private readonly string ethernaCreditBaseUrl;
        private readonly string ethernaCreditSecret;

        private readonly string ethernaDappBaseUrl;

        private readonly string ethernaGatewayCreditSecret;

        private readonly string[] ethernaGatewayWebappBaseUrls;
        private readonly string ethernaGatewayWebappSecret;

        private readonly string ethernaIndexBaseUrl;
        private readonly string ethernaIndexSecret;

        // Constructor.
        public IdServerConfig(IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            ethernaCreditBaseUrl = configuration["IdServer:Clients:EthernaCredit:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaCreditSecret = configuration["IdServer:Clients:EthernaCredit:Secret"] ?? throw new ServiceConfigurationException();

            ethernaDappBaseUrl = configuration["IdServer:Clients:EthernaDapp:BaseUrl"] ?? throw new ServiceConfigurationException();

            ethernaGatewayCreditSecret = configuration["IdServer:Clients:EthernaGatewayCreditClient:Secret"] ?? throw new ServiceConfigurationException();

            ethernaGatewayWebappBaseUrls = configuration.GetSection("IdServer:Clients:EthernaGatewayWebapp:BaseUrls").Get<string[]>() ?? throw new ServiceConfigurationException();
            ethernaGatewayWebappSecret = configuration["IdServer:Clients:EthernaGatewayWebapp:Secret"] ?? throw new ServiceConfigurationException();

            ethernaIndexBaseUrl = configuration["IdServer:Clients:EthernaIndex:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaIndexSecret = configuration["IdServer:Clients:EthernaIndex:Secret"] ?? throw new ServiceConfigurationException();
        }

        // Properties.
        public IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("ethernaCredit_serviceInteract_api", "Etherna Credit service interact API")
            };

        public IEnumerable<Client> Clients =>
            new List<Client>
            {
                //credit
                new Client
                {
                    ClientId = "ethernaCreditClientId",
                    ClientName = "Etherna Credit",
                    ClientSecrets = { new Secret(ethernaCreditSecret.Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,

                    // where to redirect to after login
                    RedirectUris = { $"{ethernaCreditBaseUrl}/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { $"{ethernaCreditBaseUrl}/signout-callback-oidc" },

                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "ether_accounts"
                    }
                },

                //dapp
                new Client
                {
                    ClientId = "ethernaDappClientId",
                    ClientName = "Etherna Dapp",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireConsent = false,
                    RequireClientSecret = false,
                    
                    // where to redirect to after login
                    RedirectUris = { $"{ethernaDappBaseUrl}/callback.html" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { ethernaDappBaseUrl },

                    AllowedCorsOrigins = { ethernaDappBaseUrl },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "ether_accounts"
                    }
                },

                //gateway validator (credit client)
                new Client
                {
                    ClientId = "ethernaGatevalCreditClientId",
                    ClientName = "Etherna Gateway Validator",
                    ClientSecrets = { new Secret(ethernaGatewayCreditSecret.Sha256()) },

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // scopes that client has access to
                    AllowedScopes =
                    {
                        "ethernaCredit_serviceInteract_api"
                    }
                },
                
                //gateway validator (user login)
                new Client
                {
                    ClientId = "ethernaGatevalUserClientId",
                    ClientName = "Etherna Gateway",
                    ClientSecrets = { new Secret(ethernaGatewayWebappSecret.Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,

                    // where to redirect to after login
                    RedirectUris = ethernaGatewayWebappBaseUrls.Select(url => $"{url}/signin-oidc").ToList(),

                    // where to redirect to after logout
                    PostLogoutRedirectUris = ethernaGatewayWebappBaseUrls.Select(url => $"{url}/signout-callback-oidc").ToList(),

                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "ether_accounts"
                    }
                },

                //index
                new Client
                {
                    ClientId = "ethernaIndexClientId",
                    ClientName = "Etherna Index",
                    ClientSecrets = { new Secret(ethernaIndexSecret.Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,

                    // where to redirect to after login
                    RedirectUris = { $"{ethernaIndexBaseUrl}/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { $"{ethernaIndexBaseUrl}/signout-callback-oidc" },

                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "ether_accounts"
                    }
                },
            };

        public IEnumerable<IdentityResource> IdResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource()
                {
                    DisplayName = "Ether accounts",
                    Name = "ether_accounts",
                    UserClaims = new List<string>()
                    {
                        UserBase.DefaultClaimTypes.EtherAddress,
                        UserBase.DefaultClaimTypes.EtherPreviousAddresses,
                        UserBase.DefaultClaimTypes.IsWeb3Account
                    }
                }
            };
    }
}
