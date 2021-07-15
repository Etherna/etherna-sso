using Hangfire;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public interface IDeleteOldInvitationsTask
    {
        [Queue(Queues.DOMAIN_MAINTENANCE)]
        Task RunAsync();
    }
}