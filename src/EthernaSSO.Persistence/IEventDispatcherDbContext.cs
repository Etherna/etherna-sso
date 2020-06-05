using Etherna.DomainEvents;
using Etherna.MongODM;

namespace Etherna.SSOServer.Persistence
{
    public interface IEventDispatcherDbContext : IDbContext
    {
        IEventDispatcher EventDispatcher { get; }
    }
}
