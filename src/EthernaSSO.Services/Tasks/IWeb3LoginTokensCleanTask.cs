using Hangfire;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public interface IWeb3LoginTokensCleanTask
    {
        [Queue(Queues.DOMAIN_MAINTENANCE)]
        Task RunAsync();
    }
}