﻿// Copyright 2021-present Etherna SA
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

using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Etherna.ACR.Exceptions;
using Etherna.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.SSOServer.Configs.IdentityServer
{
    public class IdServerConfig
    {
        // Consts.
        public static class ApiResourcesDef
        {
            public static readonly ApiResource EthernaCreditServiceInteract = new("ethernaCreditServiceInteract", "Etherna Credit service interact")
            {
                Scopes = { ApiScopesDef.EthernaCreditServiceInteract.Name }
            };

            public static readonly ApiResource EthernaSsoServiceInteract = new("ethernaSsoServiceInteract", "Etherna SSO service interact")
            {
                Scopes = { ApiScopesDef.EthernaSsoUserContactInfo.Name }
            };

            public static readonly ApiResource EthernaUserApi = new("userApi", "Etherna user APIs")
            {
                Scopes = {
                    ApiScopesDef.UserInteractEthernaCredit.Name,
                    ApiScopesDef.UserInteractEthernaGateway.Name,
                    ApiScopesDef.UserInteractEthernaIndex.Name,
                    ApiScopesDef.UserInteractEthernaSso.Name
                }
            };
        }
        public static class ApiScopesDef //these can go in very details of client permissions
        {
            //credit service interaction scopes
            public static readonly ApiScope EthernaCreditServiceInteract = new("ethernaCredit_serviceInteract_api", "Etherna Credit service interact API");

            //sso service interaction scopes
            public static readonly ApiScope EthernaSsoUserContactInfo = new("ethernaSso_userContactInfo_api", "Etherna SSO user contatct info API");

            //global user interaction scopes
            public static readonly ApiScope UserInteractEthernaCredit = new("userApi.credit", "Etherna Credit user API");
            public static readonly ApiScope UserInteractEthernaGateway = new("userApi.gateway", "Etherna Gateway user API");
            public static readonly ApiScope UserInteractEthernaIndex = new("userApi.index", "Etherna Index user API");
            public static readonly ApiScope UserInteractEthernaSso = new("userApi.sso", "Etherna Sso user API");
        }
        public static class IdResourcesDef
        {
            public static readonly IdentityResource EtherAccounts = new()
            {
                DisplayName = "Ether accounts",
                Name = "ether_accounts",
                UserClaims = new List<string>()
                {
                    EthernaClaimTypes.EtherAddress,
                    EthernaClaimTypes.EtherPreviousAddresses,
                    EthernaClaimTypes.IsWeb3Account
                }
            };
            public static readonly IdentityResource Role = new()
            {
                DisplayName = "Role",
                Name = "role",
                UserClaims = [EthernaClaimTypes.Role_IdentityModel]
            };
        }
        private const string SwaggerRedirectUriPath = "/swagger/oauth2-redirect.html";

        // Fields.
        private readonly string apiKey_ClientId;

        private readonly string ethernaCredit_BaseUrl;
        private readonly string ethernaCredit_Sso_ClientId;
        private readonly string ethernaCredit_Sso_Secret;
        private readonly string ethernaCredit_Webapp_ClientId;
        private readonly string ethernaCredit_Webapp_Secret;

        private readonly string ethernaCreditSwagger_ClientId;

        private readonly string ethernaDapp_BaseUrl;
        private readonly string ethernaDapp_ClientId;

        private readonly string[] ethernaGateway_BaseUrls;
        private readonly string ethernaGateway_Credit_ClientId;
        private readonly string ethernaGateway_Credit_Secret;
        private readonly string ethernaGateway_Webapp_ClientId;
        private readonly string ethernaGateway_Webapp_Secret;

        private readonly string ethernaGatewayCli_BaseUrl;
        private readonly string ethernaGatewayCli_ClientId;

        private readonly string ethernaGatewaySwagger_ClientId;

        private readonly string ethernaIndex_BaseUrl;
        private readonly string ethernaIndex_Sso_ClientId;
        private readonly string ethernaIndex_Sso_Secret;
        private readonly string ethernaIndex_Webapp_ClientId;
        private readonly string ethernaIndex_Webapp_Secret;

        private readonly string ethernaIndexSwagger_ClientId;

        private readonly string ethernaSso_BaseUrl;
        private readonly string ethernaSso_Webapp_ClientId;
        private readonly string ethernaSso_Webapp_Secret;

        private readonly string ethernaSsoSwagger_ClientId;

        private readonly string ethernaVideoImporter_BaseUrl;
        private readonly string ethernaVideoImporter_ClientId;

        // Constructor.
        public IdServerConfig(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            apiKey_ClientId = configuration["IdServer:Clients:ApiKey:ClientId"] ?? throw new ServiceConfigurationException();

            ethernaCredit_BaseUrl = configuration["IdServer:Clients:EthernaCredit:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaCredit_Sso_ClientId = configuration["IdServer:Clients:EthernaCredit:Clients:SsoServer:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaCredit_Sso_Secret = configuration["IdServer:Clients:EthernaCredit:Clients:SsoServer:Secret"] ?? throw new ServiceConfigurationException();
            ethernaCredit_Webapp_ClientId = configuration["IdServer:Clients:EthernaCredit:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaCredit_Webapp_Secret = configuration["IdServer:Clients:EthernaCredit:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

            ethernaCreditSwagger_ClientId = configuration["IdServer:Clients:EthernaCreditSwagger:ClientId"] ?? throw new ServiceConfigurationException();

            ethernaDapp_BaseUrl = configuration["IdServer:Clients:EthernaDapp:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaDapp_ClientId = configuration["IdServer:Clients:EthernaDapp:ClientId"] ?? throw new ServiceConfigurationException();

            ethernaGateway_BaseUrls = configuration.GetSection("IdServer:Clients:EthernaGateway:BaseUrls").Get<string[]>() ?? throw new ServiceConfigurationException();
            ethernaGateway_Credit_ClientId = configuration["IdServer:Clients:EthernaGateway:Clients:Credit:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaGateway_Credit_Secret = configuration["IdServer:Clients:EthernaGateway:Clients:Credit:Secret"] ?? throw new ServiceConfigurationException();
            ethernaGateway_Webapp_ClientId = configuration["IdServer:Clients:EthernaGateway:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaGateway_Webapp_Secret = configuration["IdServer:Clients:EthernaGateway:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

            ethernaGatewayCli_BaseUrl = configuration["IdServer:Clients:EthernaGatewayCli:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaGatewayCli_ClientId = configuration["IdServer:Clients:EthernaGatewayCli:ClientId"] ?? throw new ServiceConfigurationException();

            ethernaGatewaySwagger_ClientId = configuration["IdServer:Clients:EthernaGatewaySwagger:ClientId"] ?? throw new ServiceConfigurationException();

            ethernaIndex_BaseUrl = configuration["IdServer:Clients:EthernaIndex:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaIndex_Sso_ClientId = configuration["IdServer:Clients:EthernaIndex:Clients:SsoServer:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaIndex_Sso_Secret = configuration["IdServer:Clients:EthernaIndex:Clients:SsoServer:Secret"] ?? throw new ServiceConfigurationException();
            ethernaIndex_Webapp_ClientId = configuration["IdServer:Clients:EthernaIndex:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaIndex_Webapp_Secret = configuration["IdServer:Clients:EthernaIndex:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

            ethernaIndexSwagger_ClientId = configuration["IdServer:Clients:EthernaIndexSwagger:ClientId"] ?? throw new ServiceConfigurationException();

            ethernaSso_BaseUrl = configuration["IdServer:SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaSso_Webapp_ClientId = configuration["IdServer:SsoServer:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
            ethernaSso_Webapp_Secret = configuration["IdServer:SsoServer:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

            ethernaSsoSwagger_ClientId = configuration["IdServer:Clients:EthernaSsoSwagger:ClientId"] ?? throw new ServiceConfigurationException();

            ethernaVideoImporter_BaseUrl = configuration["IdServer:Clients:EthernaVideoImporter:BaseUrl"] ?? throw new ServiceConfigurationException();
            ethernaVideoImporter_ClientId = configuration["IdServer:Clients:EthernaVideoImporter:ClientId"] ?? throw new ServiceConfigurationException();
        }

        // Properties.
        public IEnumerable<ApiResource> ApiResources =>
        [
            ApiResourcesDef.EthernaCreditServiceInteract,
            ApiResourcesDef.EthernaSsoServiceInteract,
            ApiResourcesDef.EthernaUserApi
        ];

        public IEnumerable<ApiScope> ApiScopes =>
        [
            ApiScopesDef.EthernaCreditServiceInteract,
            ApiScopesDef.EthernaSsoUserContactInfo,
            ApiScopesDef.UserInteractEthernaCredit,
            ApiScopesDef.UserInteractEthernaGateway,
            ApiScopesDef.UserInteractEthernaIndex,
            ApiScopesDef.UserInteractEthernaSso
        ];

        public IEnumerable<Client> Clients =>
        [
            //api key
            new()
            {
                ClientId = apiKey_ClientId,
                ClientName = "Api Key client",
                RequireClientSecret = false,

                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name,

                    //resource
                    ApiScopesDef.UserInteractEthernaCredit.Name,
                    ApiScopesDef.UserInteractEthernaGateway.Name,
                    ApiScopesDef.UserInteractEthernaIndex.Name,
                    ApiScopesDef.UserInteractEthernaSso.Name,
                },

                // Allow token refresh.
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly //because client have not secret
            },

            //credit (sso client)
            new()
            {
                ClientId = ethernaCredit_Sso_ClientId,
                ClientName = "EthernaCredit Sso client",
                ClientSecrets = { new Secret(ethernaCredit_Sso_Secret.Sha256()) },

                //no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                //scopes that client has access to
                AllowedScopes =
                {
                    //resource
                    ApiScopesDef.EthernaSsoUserContactInfo.Name
                }
            },

            //credit (user login)
            new()
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
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name
                },

                // Allow token refresh.
                AllowOfflineAccess = true
            },
            
            //credit swagger
            new()
            {
                ClientId = ethernaCreditSwagger_ClientId,
                ClientName = "Etherna Credit API Swagger",
                RequireClientSecret = false,
                
                AllowedGrantTypes = GrantTypes.Code,
                
                //where to redirect to after login
                RedirectUris = { $"{ethernaCredit_BaseUrl}{SwaggerRedirectUriPath}" },
                
                AllowedCorsOrigins = { ethernaCredit_BaseUrl },
                RequirePkce = true,
                
                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name,
                    
                    //resource
                    ApiScopesDef.UserInteractEthernaCredit.Name,
                },
                
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly //because client have not secret
            },

            //dapp
            new()
            {
                ClientId = ethernaDapp_ClientId,
                ClientName = "Etherna Dapp",
                RequireClientSecret = false,

                AllowedGrantTypes = GrantTypes.Code,
                
                //where to redirect to after login
                RedirectUris = { $"{ethernaDapp_BaseUrl}/callback" },

                //where to redirect to after logout
                PostLogoutRedirectUris = { ethernaDapp_BaseUrl },

                AllowedCorsOrigins = { ethernaDapp_BaseUrl },

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name,
                    
                    //resource
                    ApiScopesDef.UserInteractEthernaCredit.Name,
                    ApiScopesDef.UserInteractEthernaGateway.Name,
                    ApiScopesDef.UserInteractEthernaIndex.Name,
                    ApiScopesDef.UserInteractEthernaSso.Name
                },

                // Allow token refresh.
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly //because client have not secret
            },

            //gateway (credit client)
            new()
            {
                ClientId = ethernaGateway_Credit_ClientId,
                ClientName = "EthernaGateway Credit client",
                ClientSecrets = { new Secret(ethernaGateway_Credit_Secret.Sha256()) },

                //no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                //scopes that client has access to
                AllowedScopes =
                {
                    //resource
                    ApiScopesDef.EthernaCreditServiceInteract.Name
                }
            },
                
            //gateway (user login)
            new()
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
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name
                },

                // Allow token refresh.
                AllowOfflineAccess = true
            },

            //gateway cli
            new()
            {
                ClientId = ethernaGatewayCli_ClientId,
                ClientName = "Etherna Gateway CLI",
                RequireClientSecret = false,

                AllowedGrantTypes = GrantTypes.Code,

                //where to redirect to after login
                RedirectUris = { ethernaGatewayCli_BaseUrl },

                //where to redirect to after logout
                PostLogoutRedirectUris = { ethernaGatewayCli_BaseUrl },

                AllowedCorsOrigins = { ethernaGatewayCli_BaseUrl },

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,

                    //resource
                    ApiScopesDef.UserInteractEthernaGateway.Name,
                },

                // Allow token refresh.
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly //because client have not secret
            },
            
            //gateway swagger
            new()
            {
                ClientId = ethernaGatewaySwagger_ClientId,
                ClientName = "Etherna Gateway API Swagger",
                RequireClientSecret = false,
                
                AllowedGrantTypes = GrantTypes.Code,
                
                //where to redirect to after login
                RedirectUris = ethernaGateway_BaseUrls.Select(url => $"{url}{SwaggerRedirectUriPath}").ToList(),
                
                AllowedCorsOrigins = ethernaGateway_BaseUrls,
                RequirePkce = true,
                
                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name,
                    
                    //resource
                    ApiScopesDef.UserInteractEthernaGateway.Name,
                },
                
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly //because client have not secret
            },
            
            //index (sso client)
            new()
            {
                ClientId = ethernaIndex_Sso_ClientId,
                ClientName = "EthernaIndex Sso client",
                ClientSecrets = { new Secret(ethernaIndex_Sso_Secret.Sha256()) },

                //no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                //scopes that client has access to
                AllowedScopes =
                {
                    //resource
                    ApiScopesDef.EthernaSsoUserContactInfo.Name
                }
            },

            //index (user login)
            new()
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
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name
                },

                // Allow token refresh.
                AllowOfflineAccess = true
            },
            
            //index swagger
            new()
            {
                ClientId = ethernaIndexSwagger_ClientId,
                ClientName = "Etherna Index API Swagger",
                RequireClientSecret = false,
                
                AllowedGrantTypes = GrantTypes.Code,
                
                //where to redirect to after login
                RedirectUris = { $"{ethernaIndex_BaseUrl}{SwaggerRedirectUriPath}" },
                
                AllowedCorsOrigins = { ethernaIndex_BaseUrl },
                RequirePkce = true,
                
                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name,
                    
                    //resource
                    ApiScopesDef.UserInteractEthernaIndex.Name,
                },
                
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly //because client have not secret
            },

            //sso (user login)
            new()
            {
                ClientId = ethernaSso_Webapp_ClientId,
                ClientName = "Etherna SSO",
                ClientSecrets = { new Secret(ethernaSso_Webapp_Secret.Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                //where to redirect to after login
                RedirectUris = { $"{ethernaSso_BaseUrl}/signin-oidc" },

                //where to redirect to after logout
                PostLogoutRedirectUris = { $"{ethernaSso_BaseUrl}/signout-callback-oidc" },

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name
                },

                // Allow token refresh.
                AllowOfflineAccess = true
            },
            
            //sso swagger
            new()
            {
                ClientId = ethernaSsoSwagger_ClientId,
                ClientName = "Etherna SSO API Swagger",
                RequireClientSecret = false,
                
                AllowedGrantTypes = GrantTypes.Code,
                
                //where to redirect to after login
                RedirectUris = { $"{ethernaSso_BaseUrl}{SwaggerRedirectUriPath}" },
                
                AllowedCorsOrigins = { ethernaSso_BaseUrl },
                RequirePkce = true,
                
                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,
                    IdResourcesDef.Role.Name,
                    
                    //resource
                    ApiScopesDef.UserInteractEthernaSso.Name,
                },
                
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly //because client have not secret
            },

            //video importer
            new()
            {
                ClientId = ethernaVideoImporter_ClientId,
                ClientName = "Etherna Video Importer",
                RequireClientSecret = false,

                AllowedGrantTypes = GrantTypes.Code,

                //where to redirect to after login
                RedirectUris = { ethernaVideoImporter_BaseUrl },

                //where to redirect to after logout
                PostLogoutRedirectUris = { ethernaVideoImporter_BaseUrl },

                AllowedCorsOrigins = { ethernaVideoImporter_BaseUrl },

                AlwaysIncludeUserClaimsInIdToken = true,
                AllowedScopes =
                {
                    //identity
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdResourcesDef.EtherAccounts.Name,

                    //resource
                    ApiScopesDef.UserInteractEthernaGateway.Name,
                    ApiScopesDef.UserInteractEthernaIndex.Name,
                },

                // Allow token refresh.
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly //because client have not secret
            }
        ];

        public IEnumerable<IdentityResource> IdResources => new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            IdResourcesDef.EtherAccounts,
            IdResourcesDef.Role
        };
    }
}
