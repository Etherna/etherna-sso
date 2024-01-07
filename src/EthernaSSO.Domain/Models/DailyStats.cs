// Copyright 2021-present Etherna Sa
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
