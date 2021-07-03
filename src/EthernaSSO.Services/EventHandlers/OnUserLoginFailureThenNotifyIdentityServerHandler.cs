using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using IdentityServer4.Services;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnUserLoginFailureThenNotifyIdentityServerHandler : EventHandlerBase<UserLoginFailureEvent>
    {
        // Fields.
        private readonly IEventService identityServerEventService;

        // Constructor.
        public OnUserLoginFailureThenNotifyIdentityServerHandler(
            IEventService identityServerEventService)
        {
            this.identityServerEventService = identityServerEventService;
        }

        // Methods.
        public override async Task HandleAsync(UserLoginFailureEvent @event)
        {
            await identityServerEventService.RaiseAsync(new IdentityServer4.Events.UserLoginFailureEvent(
                @event.Identifier,
                @event.Error,
                clientId: @event.ClientId));
        }
    }
}
