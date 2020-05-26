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
        private readonly string ethernaIndexBaseUrl;
        private readonly string ethernaIndexSecret;

        // Constructor.
        public IdServerConfig(IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

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

        public IEnumerable<Client> Clients => //will disappear client authentication
            new List<Client>
            {
                // interactive ASP.NET Core MVC client
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

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                }
            };

        public IEnumerable<IdentityResource> IdResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
    }
}
