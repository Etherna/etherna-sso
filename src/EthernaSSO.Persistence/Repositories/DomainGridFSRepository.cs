using Digicando.DomainEvents;
using Digicando.MongODM;
using Digicando.MongODM.Models;
using Digicando.MongODM.Repositories;
using Etherna.SSOServer.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Persistence.Repositories
{
    public class DomainGridFSRepository<TModel> :
        GridFSRepository<TModel>
        where TModel : class, IFileModel
    {
        // Constructors and initialization.
        public DomainGridFSRepository(string name)
            : base(name)
        { }

        public DomainGridFSRepository(GridFSRepositoryOptions<TModel> options)
            : base(options)
        { }

        public override void Initialize(IDbContext dbContext)
        {
            if (!(dbContext is IEventDispatcherDbContext))
                throw new InvalidOperationException($"DbContext needs to implement {nameof(IEventDispatcherDbContext)}");

            base.Initialize(dbContext);
        }

        // Properties.
        public IEventDispatcher EventDispatcher => ((IEventDispatcherDbContext)DbContext).EventDispatcher;

        // Methods.
        public override async Task CreateAsync(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            await base.CreateAsync(models, cancellationToken);

            // Dispatch created events.
            await EventDispatcher.DispatchAsync(
                models.Select(m => new EntityCreatedEvent<TModel>(m)));
        }

        public override async Task CreateAsync(TModel model, CancellationToken cancellationToken = default)
        {
            await base.CreateAsync(model);

            // Dispatch created event.
            await EventDispatcher.DispatchAsync(
                new EntityCreatedEvent<TModel>(model));
        }

        public override async Task DeleteAsync(TModel model, CancellationToken cancellationToken = default)
        {
            await base.DeleteAsync(model, cancellationToken);

            // Dispatch deleted event.
            await EventDispatcher.DispatchAsync(
                new EntityDeletedEvent<TModel>(model));
        }
    }
}
