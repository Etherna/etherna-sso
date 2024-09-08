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

using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain.Models.UserAgg;

namespace Etherna.SSOServer.Persistence.ModelMaps.Shared
{
    internal sealed class UserSharedInfoMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<UserSharedInfo>("6d0d2ee1-6aa3-42ea-9833-ac592bfc6613", mm =>
            {
                mm.AutoMap();

                // Set members to ignore if null or default.
                mm.GetMemberMap(u => u.LockoutEnd).SetIgnoreIfNull(true);
            });
        }
    }
}
