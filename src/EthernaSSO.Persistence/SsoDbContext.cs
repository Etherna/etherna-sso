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
    public class SsoDbContext(
        IEventDispatcher eventDispatcher,
        SsoDbSeedSettings seedSettings,
        IServiceProvider serviceProvider)
        : DbContext, IEventDispatcherDbContext, ISsoDbContext
    {
        // Consts.
        private const string ModelMapsNamespace = "Etherna.SSOServer.Persistence.ModelMaps.Sso";

        // Properties.
        //repositories
        public IRepository<AlphaPassRequest, string> AlphaPassRequests { get; } = new DomainRepository<AlphaPassRequest, string>(
            new RepositoryOptions<AlphaPassRequest>("alphaPassRequests")
            {
                IndexBuilders = new[]
                {
                    (Builders<AlphaPassRequest>.IndexKeys.Ascending(r => r.NormalizedEmail),
                     new CreateIndexOptions<AlphaPassRequest> { Unique = true }),

                    (Builders<AlphaPassRequest>.IndexKeys.Ascending(r => r.CreationDateTime)
                                                         .Ascending(r => r.IsEmailConfirmed)
                                                         .Ascending(r => r.IsInvitationSent),
                     new CreateIndexOptions<AlphaPassRequest>())
                }
            });
        public IRepository<ApiKey, string> ApiKeys { get; } = new DomainRepository<ApiKey, string>(
            new RepositoryOptions<ApiKey>("apiKeys")
            {
                IndexBuilders = new[]
                {
                    (Builders<ApiKey>.IndexKeys.Ascending(k => k.KeyHash),
                     new CreateIndexOptions<ApiKey> { Unique = true })
                }
            });
        public IRepository<DailyStats, string> DailyStats { get; } = new DomainRepository<DailyStats, string>("dailyStats");
        public IRepository<Invitation, string> Invitations { get; } = new DomainRepository<Invitation, string>(
            new RepositoryOptions<Invitation>("invitations")
            {
                IndexBuilders = new[]
                {
                    (Builders<Invitation>.IndexKeys.Ascending(i => i.Code),
                     new CreateIndexOptions<Invitation> { Unique = true })
                }
            });
        public IRepository<Role, string> Roles { get; } = new DomainRepository<Role, string>(
            new RepositoryOptions<Role>("roles")
            {
                IndexBuilders = new[]
                {
                    (Builders<Role>.IndexKeys.Ascending(r => r.NormalizedName),
                     new CreateIndexOptions<Role> { Unique = true })
                }
            });
        public IRepository<UserBase, string> Users { get; } = new DomainRepository<UserBase, string>(
            new RepositoryOptions<UserBase>("users")
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
        public IRepository<Web3LoginToken, string> Web3LoginTokens { get; } = new DomainRepository<Web3LoginToken, string>(
            new RepositoryOptions<Web3LoginToken>("web3LoginTokens")
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
        public IEventDispatcher EventDispatcher { get; } = eventDispatcher;

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(SsoDbContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == ModelMapsNamespace
            where t.GetInterfaces().Contains(typeof(IModelMapsCollector))
            select Activator.CreateInstance(t) as IModelMapsCollector;

        // Methods.
        public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changedEntityModels = ChangedModelsList.OfType<EntityModelBase>().ToArray();

            // Save changes.
            await base.SaveChangesAsync(cancellationToken);

            // Dispatch events.
            foreach (var model in changedEntityModels)
            {
                await EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }
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
                    [adminRole],
                    false);

                if (user is null)
                    throw new InvalidOperationException("Error creating first user");
            }
        }
    }
}
