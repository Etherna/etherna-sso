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
using Etherna.MongODM.Core.Migration;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Persistence.Repositories;
using Etherna.SSOServer.Persistence.Settings;
using Etherna.SSOServer.Services.Domain;
using Microsoft.Extensions.DependencyInjection;
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
        private const string ModelMapsNamespace = "Etherna.SSOServer.Persistence.ModelMaps.Sso";

        // Fields.
        private readonly SsoDbSeedSettings seedSettings;
        private readonly IServiceProvider serviceProvider;

        // Constructor.
        public SsoDbContext(
            IEventDispatcher eventDispatcher,
            SsoDbSeedSettings seedSettings,
            IServiceProvider serviceProvider)
        {
            EventDispatcher = eventDispatcher;
            this.seedSettings = seedSettings;
            this.serviceProvider = serviceProvider;
        }

        // Properties.
        //repositories
        public ICollectionRepository<AlphaPassRequest, string> AlphaPassRequests { get; } = new DomainCollectionRepository<AlphaPassRequest, string>(
            new CollectionRepositoryOptions<AlphaPassRequest>("alphaPassRequests")
            {
                IndexBuilders = new[]
                {
                    (Builders<AlphaPassRequest>.IndexKeys.Ascending(r => r.NormalizedEmail),
                     new CreateIndexOptions<AlphaPassRequest> { Unique = true })
                }
            });
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
                     new CreateIndexOptions<UserBase>()),

                    (Builders<UserBase>.IndexKeys.Descending(u => u.LastLoginDateTime),
                     new CreateIndexOptions<UserBase> { Sparse = true }),

                    (Builders<UserBase>.IndexKeys.Ascending(u => u.NormalizedEmail),
                     new CreateIndexOptions<UserBase> { Unique = true, Sparse = true }),

                    (Builders<UserBase>.IndexKeys.Ascending(u => u.NormalizedUsername),
                     new CreateIndexOptions<UserBase> { Unique = true }),

                    (Builders<UserBase>.IndexKeys.Ascending("Roles.NormalizedName"),
                     new CreateIndexOptions<UserBase>()),

                    (Builders<UserBase>.IndexKeys.Ascending(u => u.SharedInfoId),
                     new CreateIndexOptions<UserBase> { Unique = true }),

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

        // Protected methods.
        protected override async Task SeedAsync()
        {
            using (EventDispatcher.DisableEventDispatch())
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var userService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();

                // Create admin role.
                var adminRole = new Role(Role.AdministratorName);
                await Roles.CreateAsync(adminRole);

                // Create admin user.
                var (_, user) = await userService.RegisterWeb2UserByAdminAsync(
                    seedSettings.FirstAdminUsername,
                    seedSettings.FirstAdminPassword,
                    null,
                    null,
                    true,
                    null,
                    null,
                    new[] { adminRole },
                    false);

                if (user is null)
                    throw new InvalidOperationException("Error creating first user");
            }
        }
    }
}
