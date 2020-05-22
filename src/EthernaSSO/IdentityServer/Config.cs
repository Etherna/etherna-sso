using IdentityServer4.Models;
using System.Collections.Generic;

namespace Etherna.SSOServer.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource("ethernaIndexApiAdmin", "Etherna Index API Administrator"),
                new ApiResource("ethernaIndexApiDefault", "Etherna Index API Default")
            };

        public static IEnumerable<Client> Clients => //will disappear client authentication
            new List<Client>
            {
                new Client
                {
                    ClientId = "client",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = { "ethernaIndexApiDefault" }
                }
            };
    }
}
