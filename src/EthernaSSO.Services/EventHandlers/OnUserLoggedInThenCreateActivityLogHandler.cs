using Digicando.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnUserLoggedInThenCreateActivityLogHandler : EventHandlerBase<UserLoggedInEvent>
    {
        public override Task HandleAsync(UserLoggedInEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}
