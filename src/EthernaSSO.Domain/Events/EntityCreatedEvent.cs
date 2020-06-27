using Etherna.DomainEvents;
using Etherna.MongODM.Models;

namespace Etherna.SSOServer.Domain.Events
{
    public class EntityCreatedEvent<TModel> : IDomainEvent
        where TModel : IEntityModel
    {
        public EntityCreatedEvent(TModel entity)
        {
            Entity = entity;
        }

        public TModel Entity { get; }
    }
}
