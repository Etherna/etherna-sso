using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.IdentityServer
{
    public class IdServerConfig
    {
        // Fields.
        private readonly string ethernaDappBaseUrl;
        private readonly string ethernaIndexBaseUrl;
        private readonly string ethernaIndexSecret;

        // Constructor.
        public IdServerConfig(IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            ethernaDappBaseUrl = configuration["IdServer:Clients:EthernaDapp:BaseUrl"];

            ethernaIndexBaseUrl = configuration["IdServer:Clients:EthernaIndex:BaseUrl"];
            ethernaIndexSecret = configuration["IdServer:Clients:EthernaIndex:Secret"];
        }

        // Properties.
        public IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource("ethernaIndexApi", "Etherna Index API"),
                new ApiResource("ethernaGatewayApi", "Etherna Gateway API")
            };

        public IEnumerable<Client> Clients =>
            new List<Client>
            {
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
                    PostLogoutRedirectUris = { $"{ethernaDappBaseUrl}/index.html" },

                    AllowedCorsOrigins = { ethernaDappBaseUrl },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
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
                        "ether_accounts"
                    }
                }
            };

        public IEnumerable<IdentityResource> IdResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResource()
                {
                    DisplayName = "Ether accounts",
                    Name = "ether_accounts",
                    UserClaims = new List<string>()
                    {
                        "ether_address",
                        "ether_prev_addresses"
                    }
                }
            };
    }
}
