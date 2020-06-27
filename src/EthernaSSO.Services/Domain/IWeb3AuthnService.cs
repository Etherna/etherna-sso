using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    public interface IWeb3AuthnService
    {
        string ComposeAuthMessage(string code);
        Task<string> RetriveAuthnMessageAsync(string etherAddress);
        bool VerifySignature(string authCode, string etherAccount, string signature);
    }
}