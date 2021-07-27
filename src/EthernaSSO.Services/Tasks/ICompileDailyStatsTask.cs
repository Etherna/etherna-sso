using Hangfire;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public interface ICompileDailyStatsTask
    {
        [Queue(Queues.STATS)]
        Task RunAsync();
    }
}