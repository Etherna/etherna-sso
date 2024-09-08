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

namespace Etherna.SSOServer.Domain.Models
{
    public class DailyStats : EntityModelBase<string>
    {
        // Constructors.
        public DailyStats(
            long activeUsersInLast30Days,
            long activeUsersInLast60Days,
            long activeUsersInLast180Days,
            long totalUsers)
        {
            ActiveUsersInLast30Days = activeUsersInLast30Days;
            ActiveUsersInLast60Days = activeUsersInLast60Days;
            ActiveUsersInLast180Days = activeUsersInLast180Days;
            TotalUsers = totalUsers;
        }
        protected DailyStats() { }

        // Properties.
        public virtual long ActiveUsersInLast30Days { get; protected set; }
        public virtual long ActiveUsersInLast60Days { get; protected set; }
        public virtual long ActiveUsersInLast180Days { get; protected set; }
        public virtual long TotalUsers { get; protected set; }
    }
}
