// Copyright 2021-present Etherna SA
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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SwarmSdk.Models;
using Nethereum.Signer;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    internal sealed class Web3AuthnService(ISsoDbContext ssoDbContext) : IWeb3AuthnService
    {
        // Methods.
        public string ComposeAuthMessage(string code) =>
            $"Sign this message to verify your address with Etherna! Code: {code}";

        public async Task<string> RetrieveAuthnMessageAsync(EthAddress etherAddress)
        {
            var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            if (token is null)
            {
                token = new Web3LoginToken(etherAddress);
                await ssoDbContext.Web3LoginTokens.CreateAsync(token);
            }

            return ComposeAuthMessage(token.Code);
        }

        public bool VerifySignature(string authCode, EthAddress etherAccount, string signature)
        {
            var message = ComposeAuthMessage(authCode);

            var signer = new EthereumMessageSigner();
            var recAddress = signer.EncodeUTF8AndEcRecover(message, signature);

            return EthAddress.FromString(recAddress) == etherAccount;
        }
    }
}
