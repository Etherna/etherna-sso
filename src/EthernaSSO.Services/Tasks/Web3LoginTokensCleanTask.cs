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

using Etherna.MongoDB.Driver;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public sealed class Web3LoginTokensCleanTask : IWeb3LoginTokensCleanTask
    {
        // Consts.
        public const string TaskId = "web3LoginTokensCleanTask";

        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructor.
        public Web3LoginTokensCleanTask(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Methods.
        public Task RunAsync() =>
            ssoDbContext.Web3LoginTokens.AccessToCollectionAsync(collection =>
                collection.DeleteManyAsync(
                    Builders<Web3LoginToken>.Filter.Where(t => t.CreationDateTime < DateTime.UtcNow - TimeSpan.FromDays(1))));
    }
}
