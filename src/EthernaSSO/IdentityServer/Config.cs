using IdentityServer4.Models;
using System.Collections.Generic;

namespace Etherna.SSOServer.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource("ethernaAdminApi", "Etherna Admin API"),
                new ApiResource("ethernaUserApi", "Etherna User API")
            };

        public static IEnumerable<Client> Clients => //will disappear client authentication
            new List<Client>
            {
                new Client
                {
                    ClientId = "ethernaIndex",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("ethernaIndexSecret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = { "ethernaUserApi" }
                },
            };

        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
    }
}
