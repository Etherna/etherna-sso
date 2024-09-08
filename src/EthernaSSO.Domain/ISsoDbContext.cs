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
using Etherna.MongODM.Core.Repositories;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Domain
{
    public interface ISsoDbContext : IDbContext
    {
        IRepository<AlphaPassRequest, string> AlphaPassRequests { get; }
        IRepository<ApiKey, string> ApiKeys { get; }
        IRepository<DailyStats, string> DailyStats { get; }
        IRepository<Invitation, string> Invitations { get; }
        IRepository<Role, string> Roles { get; }
        IRepository<UserBase, string> Users { get; }
        IRepository<Web3LoginToken, string> Web3LoginTokens { get; }
    }
}
