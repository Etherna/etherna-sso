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

using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
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
