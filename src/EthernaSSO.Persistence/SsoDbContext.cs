using Etherna.DomainEvents;
using Etherna.MongODM;
using Etherna.MongODM.Repositories;
using Etherna.MongODM.Serialization;
using Etherna.MongODM.Utility;
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
        private const string SerializersNamespace = "Etherna.SSOServer.Persistence.ClassMaps";

        // Constructor.
        public SsoDbContext(
            IDbContextDependencies dbContextDependencies,
            IEventDispatcher eventDispatcher,
            DbContextOptions<SsoDbContext> options)
            : base(dbContextDependencies, options)
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
                    (Builders<User>.IndexKeys.Ascending(u => u.EtherLoginAddress),
                     new CreateIndexOptions<User>{ Unique = true, Sparse = true }),

                    (Builders<User>.IndexKeys.Ascending(u => u.Logins),
                     new CreateIndexOptions<User>{ Unique = true, Sparse = true }),

                    (Builders<User>.IndexKeys.Ascending(u => u.NormalizedEmail),
                     new CreateIndexOptions<User> { Unique = true, Sparse = true }),

                    (Builders<User>.IndexKeys.Ascending(u => u.NormalizedUsername),
                     new CreateIndexOptions<User> { Unique = true, Sparse = true })
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

        //other properties
        public IEventDispatcher EventDispatcher { get; }

        // Protected properties.
        protected override IEnumerable<IModelSerializerCollector> SerializerCollectors =>
            from t in typeof(SsoDbContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == SerializersNamespace
            where t.GetInterfaces().Contains(typeof(IModelSerializerCollector))
            select Activator.CreateInstance(t) as IModelSerializerCollector;

        // Methods.
        public override Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch events.
            foreach (var model in ChangedModelsList.Select(m => (EntityModelBase)m))
            {
                EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
