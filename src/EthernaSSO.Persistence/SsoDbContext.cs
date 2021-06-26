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
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Migration;
using Etherna.MongODM.Core.Options;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Persistence.Repositories;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Persistence
{
    public class SsoDbContext : DbContext, IEventDispatcherDbContext, ISsoDbContext
    {
        // Consts.
        private const string SerializersNamespace = "Etherna.SSOServer.Persistence.ModelMaps";

        // Constructor.
        public SsoDbContext(
            IDbDependencies dbDependencies,
            IEventDispatcher eventDispatcher,
            DbContextOptions<SsoDbContext> options)
            : base(dbDependencies, options)
        {
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public ICollectionRepository<User, string> Users { get; } = new DomainCollectionRepository<User, string>(
            new CollectionRepositoryOptions<User>("users")
            {
                IndexBuilders = new[]
                {
                    (Builders<User>.IndexKeys.Ascending(u => u.EtherAddress),
                     new CreateIndexOptions<User>{ Unique = true }),

                    (Builders<User>.IndexKeys.Ascending(u => u.EtherLoginAddress),
                     new CreateIndexOptions<User>{ Unique = true, Sparse = true }),

                    (Builders<User>.IndexKeys.Ascending("Logins.LoginProvider")
                                             .Ascending("Logins.ProviderKey"),
                     new CreateIndexOptions<User>{ Unique = true, Sparse = true }),

                    (Builders<User>.IndexKeys.Ascending(u => u.NormalizedEmail),
                     new CreateIndexOptions<User> { Unique = true, Sparse = true }),

                    (Builders<User>.IndexKeys.Ascending(u => u.NormalizedUsername),
                     new CreateIndexOptions<User> { Unique = true })
                }
            });
        public ICollectionRepository<Web3LoginToken, string> Web3LoginTokens { get; } = new DomainCollectionRepository<Web3LoginToken, string>(
            new CollectionRepositoryOptions<Web3LoginToken>("web3LoginTokens")
            {
                IndexBuilders = new[]
                {
                    (Builders<Web3LoginToken>.IndexKeys.Ascending(u => u.EtherAddress),
                     new CreateIndexOptions<Web3LoginToken> { Unique = true })
                }
            });

        //migrations
        public override IEnumerable<DocumentMigration> DocumentMigrationList => Array.Empty<DocumentMigration>();

        //other properties
        public IEventDispatcher EventDispatcher { get; }

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(SsoDbContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == SerializersNamespace
            where t.GetInterfaces().Contains(typeof(IModelMapsCollector))
            select Activator.CreateInstance(t) as IModelMapsCollector;

        // Methods.
        public override Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch events.
            foreach (var model in ChangedModelsList.Where(m => m is EntityModelBase)
                                                   .Select(m => (EntityModelBase)m))
            {
                EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
