using Digicando.DomainEvents;
using Digicando.MongODM;

namespace Etherna.SSOServer.Persistence
{
    public interface IEventDispatcherDbContext : IDbContext
    {
        IEventDispatcher EventDispatcher { get; }
    }
}
