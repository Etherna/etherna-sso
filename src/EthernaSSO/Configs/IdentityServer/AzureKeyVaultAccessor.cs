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

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Etherna.SSOServer.Configs.IdentityServer
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
