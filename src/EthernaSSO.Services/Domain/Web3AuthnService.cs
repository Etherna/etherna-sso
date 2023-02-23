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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Nethereum.Signer;
using Nethereum.Util;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    internal sealed class Web3AuthnService : IWeb3AuthnService
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructor.
        public Web3AuthnService(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Methods.
        public string ComposeAuthMessage(string code) =>
            $"Sign this message for verify your address with Etherna! Code: {code}";

        public async Task<string> RetriveAuthnMessageAsync(string etherAddress)
        {
            var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            if (token is null)
            {
                token = new Web3LoginToken(etherAddress);
                await ssoDbContext.Web3LoginTokens.CreateAsync(token);
            }

            return ComposeAuthMessage(token.Code);
        }

        public bool VerifySignature(string authCode, string etherAccount, string signature)
        {
            var message = ComposeAuthMessage(authCode);

            var signer = new EthereumMessageSigner();
            var recAddress = signer.EncodeUTF8AndEcRecover(message, signature);

            return recAddress.IsTheSameAddress(etherAccount);
        }
    }
}
