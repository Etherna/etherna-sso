using Hangfire;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public interface IProcessAlphaPassRequestsTask
    {
        [Queue(Queues.DOMAIN_MAINTENANCE)]
        Task RunAsync();
    }
}