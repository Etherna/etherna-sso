using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using IdentityServer4.Services;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnUserLogoutSuccessThenNotifyIdentityServerHandler : EventHandlerBase<UserLogoutSuccessEvent>
    {
        // Fields.
        private readonly IEventService identityServerEventService;

        // Constructor.
        public OnUserLogoutSuccessThenNotifyIdentityServerHandler(
            IEventService identityServerEventService)
        {
            this.identityServerEventService = identityServerEventService;
        }

        // Methods.
        public override async Task HandleAsync(UserLogoutSuccessEvent @event)
        {
            await identityServerEventService.RaiseAsync(new IdentityServer4.Events.UserLogoutSuccessEvent(
                @event.User.Id,
                @event.User.Username));
        }
    }
}
