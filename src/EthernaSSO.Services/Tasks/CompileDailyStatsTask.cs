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

using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public sealed class CompileDailyStatsTask(
        ILogger<CompileDailyStatsTask> logger,
        ISsoDbContext ssoDbContext)
        : ICompileDailyStatsTask
    {
        // Consts.
        public const string TaskId = "compileDailyStatsTask";

        // Methods.
        public async Task RunAsync()
        {
            var active30d = await ssoDbContext.Users.QueryElementsAsync(users =>
                users.Where(u => u.LastLoginDateTime >= DateTime.UtcNow - TimeSpan.FromDays(30))
                     .CountAsync());

            var active60d = await ssoDbContext.Users.QueryElementsAsync(users =>
                users.Where(u => u.LastLoginDateTime >= DateTime.UtcNow - TimeSpan.FromDays(60))
                     .CountAsync());

            var active180d = await ssoDbContext.Users.QueryElementsAsync(users =>
                users.Where(u => u.LastLoginDateTime >= DateTime.UtcNow - TimeSpan.FromDays(180))
                     .CountAsync());

            var totalUsers = await ssoDbContext.Users.QueryElementsAsync(users =>
                users.CountAsync());

            await ssoDbContext.DailyStats.CreateAsync(new DailyStats(active30d, active60d, active180d, totalUsers));

            logger.DailyStatsCompiled(active30d, active60d, active180d, totalUsers);
        }
    }
}
