﻿// Copyright 2021-present Etherna Sa
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
