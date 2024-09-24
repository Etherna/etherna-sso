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
    }
}
