using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public class CompileDailyStatsTask : ICompileDailyStatsTask
    {
        // Consts.
        public const string TaskId = "compileDailyStatsTask";

        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructor.
        public CompileDailyStatsTask(ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Methods.
        public async Task RunAsync()
        {
            var stats = new DailyStats(
                await ssoDbContext.Users.QueryElementsAsync(users =>
                    users.Where(u => u.LastLoginDateTime >= DateTime.Now - TimeSpan.FromDays(30))
                         .CountAsync()),

                await ssoDbContext.Users.QueryElementsAsync(users =>
                    users.Where(u => u.LastLoginDateTime >= DateTime.Now - TimeSpan.FromDays(60))
                         .CountAsync()),

                await ssoDbContext.Users.QueryElementsAsync(users =>
                    users.Where(u => u.LastLoginDateTime >= DateTime.Now - TimeSpan.FromDays(180))
                         .CountAsync()),

                await ssoDbContext.Users.QueryElementsAsync(users =>
                    users.CountAsync()));

            await ssoDbContext.DailyStats.CreateAsync(stats);
        }
    }
}
