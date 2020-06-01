using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Etherna.SSOServer.IdentityServer
{
    public static class AzureKeyVaultAccessor
    {
        public static X509Certificate2 GetIdentityServerCertificate(IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            var keyVaultClient = new SecretClient(new Uri(configuration["AZURE_KEYVAULT_URI"]),
                                                       new ClientSecretCredential(
                                                           configuration["AZURE_TENANT_ID"],
                                                           configuration["AZURE_CLIENT_ID"],
                                                           configuration["AZURE_CLIENT_SECRET"]));

            var response = keyVaultClient.GetSecret(configuration["AZURE_KEYVAULT_CERTNAME"]);
            return new X509Certificate2(Convert.FromBase64String(response.Value.Value));
        }
    }
}
