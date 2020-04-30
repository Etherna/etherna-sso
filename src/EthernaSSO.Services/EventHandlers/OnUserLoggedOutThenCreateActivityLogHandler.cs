using Digicando.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnUserLoggedOutThenCreateActivityLogHandler : EventHandlerBase<UserLoggedOutEvent>
    {
        public override Task HandleAsync(UserLoggedOutEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}
