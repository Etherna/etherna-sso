using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Nethereum.Signer;
using Nethereum.Util;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    class Web3AuthnService : IWeb3AuthnService
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
