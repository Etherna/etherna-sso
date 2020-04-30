using Digicando.DomainEvents;
using Digicando.MongODM;
using Digicando.MongODM.Repositories;
using Digicando.MongODM.Serialization;
using Digicando.MongODM.Utility;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Persistence
{
    class SsoDbContext : DbContext, IEventDispatcherDbContext, ISsoDbContext
    {
        // Consts.
        private const string SerializersNamespace = "Etherna.SSOServer.Persistence.ClassMaps";

        // Constructor.
        public SsoDbContext(
            IDbContextDependencies dbContextDependencies,
            IEventDispatcher eventDispatcher,
            DbContextOptions options)
            : base(dbContextDependencies, options)
        {
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public ICollectionRepository<ActivityLog, string> ActivityLogs { get; } = new CollectionRepository<ActivityLog, string>("activityLogs");
        public ICollectionRepository<User, string> Users { get; } = new CollectionRepository<User, string>("users");

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
