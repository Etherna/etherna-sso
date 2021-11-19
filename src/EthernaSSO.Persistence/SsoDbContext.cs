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
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Persistence.Repositories;
using Etherna.SSOServer.Persistence.Settings;
using Microsoft.AspNetCore.Identity;
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

        // Fields.
        private readonly IPasswordHasher<UserBase> passwordHasher;
        private readonly DbSeedSettings seedSettings;

        // Constructor.
        public SsoDbContext(
            IEventDispatcher eventDispatcher,
            IPasswordHasher<UserBase> passwordHasher,
            DbSeedSettings seedSettings)
        {
            EventDispatcher = eventDispatcher;
            this.passwordHasher = passwordHasher;
            this.seedSettings = seedSettings;
        }

        // Properties.
        //repositories
        public ICollectionRepository<DailyStats, string> DailyStats { get; } = new DomainCollectionRepository<DailyStats, string>("dailyStats");
        public ICollectionRepository<Invitation, string> Invitations { get; } = new DomainCollectionRepository<Invitation, string>(
            new CollectionRepositoryOptions<Invitation>("invitations")
            {
                IndexBuilders = new[]
                {
                    (Builders<Invitation>.IndexKeys.Ascending(i => i.Code),
                     new CreateIndexOptions<Invitation> { Unique = true })
                }
            });
        public ICollectionRepository<Role, string> Roles { get; } = new DomainCollectionRepository<Role, string>(
            new CollectionRepositoryOptions<Role>("roles")
            {
                IndexBuilders = new[]
                {
                    (Builders<Role>.IndexKeys.Ascending(r => r.NormalizedName),
                     new CreateIndexOptions<Role> { Unique = true })
                }
            });
        public ICollectionRepository<UserBase, string> Users { get; } = new DomainCollectionRepository<UserBase, string>(
            new CollectionRepositoryOptions<UserBase>("users")
            {
                IndexBuilders = new[]
                {
                    //UserBase
                    (Builders<UserBase>.IndexKeys.Ascending(u => u.EtherAddress),
                     new CreateIndexOptions<UserBase> { Unique = true }),

                    (Builders<UserBase>.IndexKeys.Ascending(u => u.EtherPreviousAddresses),
                     new CreateIndexOptions<UserBase> { Unique = true }),

                    (Builders<UserBase>.IndexKeys.Descending(u => u.LastLoginDateTime),
                     new CreateIndexOptions<UserBase> { Sparse = true }),

                    (Builders<UserBase>.IndexKeys.Ascending(u => u.NormalizedEmail),
                     new CreateIndexOptions<UserBase> { Unique = true, Sparse = true }),

                    (Builders<UserBase>.IndexKeys.Ascending(u => u.NormalizedUsername),
                     new CreateIndexOptions<UserBase> { Unique = true }),

                    (Builders<UserBase>.IndexKeys.Ascending("Roles.NormalizedName"),
                     new CreateIndexOptions<UserBase>()),

                    //UserWeb2
                    (Builders<UserBase>.IndexKeys.Ascending("EtherLoginAddress"),
                     new CreateIndexOptions<UserBase> { Unique = true, Sparse = true }),

                    (Builders<UserBase>.IndexKeys.Ascending("Logins.LoginProvider")
                                                 .Ascending("Logins.ProviderKey"),
                     new CreateIndexOptions<UserBase> { Unique = true, Sparse = true }),
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

        // Protected methods.
        protected override async Task SeedAsync()
        {
            using (EventDispatcher.DisableEventDispatch())
            {
                // Create admin role.
                var adminRole = new Role(Role.AdministratorName);
                await Roles.CreateAsync(adminRole);

                // Create admin user.
                var adminUser = new UserWeb2(seedSettings.FirstAdminUsername, null, true, null);
                var pswHash = passwordHasher.HashPassword(adminUser, seedSettings.FirstAdminPassword);
                adminUser.PasswordHash = pswHash;
                adminUser.SecurityStamp = "JC6W6WKRWFN5WHOTFUX5TIKZG2KDFXQQ";
                adminUser.AddRole(adminRole);
                await Users.CreateAsync(adminUser);
            }
        }
    }
}
