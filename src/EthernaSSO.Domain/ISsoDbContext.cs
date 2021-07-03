﻿//   Copyright 2021-present Etherna Sagl
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
using Etherna.SSOServer.Domain.Models.Logs;

namespace Etherna.SSOServer.Domain
{
    public interface ISsoDbContext : IDbContext
    {
        ICollectionRepository<LogBase, string> Logs { get; }
        ICollectionRepository<Role, string> Roles { get; }
        ICollectionRepository<User, string> Users { get; }
        ICollectionRepository<Web3LoginToken, string> Web3LoginTokens { get; }
    }
}
