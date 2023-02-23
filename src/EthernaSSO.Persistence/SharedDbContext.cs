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

using Etherna.DomainEvents;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Etherna.SSOServer.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Persistence
{
    public class SharedDbContext : DbContext, IEventDispatcherDbContext, ISharedDbContext
    {
        // Consts.
        private const string ModelMapsNamespace = "Etherna.SSOServer.Persistence.ModelMaps.Shared";

        // Constructor.
        public SharedDbContext(
            IEventDispatcher eventDispatcher)
        {
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public IRepository<UserSharedInfo, string> UsersInfo { get; } = new DomainRepository<UserSharedInfo, string>(
            new RepositoryOptions<UserSharedInfo>("usersInfo")
            {
                IndexBuilders = new[]
                {
                    (Builders<UserSharedInfo>.IndexKeys.Ascending(u => u.EtherAddress),
                     new CreateIndexOptions<UserSharedInfo> { Unique = true }),

                    (Builders<UserSharedInfo>.IndexKeys.Ascending(u => u.EtherPreviousAddresses),
                     new CreateIndexOptions<UserSharedInfo>()),
                }
            });

        //other properties
        public IEventDispatcher EventDispatcher { get; }

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(SharedDbContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == ModelMapsNamespace
            where t.GetInterfaces().Contains(typeof(IModelMapsCollector))
            select Activator.CreateInstance(t) as IModelMapsCollector;

        // Methods.
        public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch events.
            foreach (var model in ChangedModelsList.Where(m => m is EntityModelBase)
                                                   .Select(m => (EntityModelBase)m)
                                                   .ToArray())
            {
                await EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }

            await base.SaveChangesAsync(cancellationToken);
        }
    }
}
