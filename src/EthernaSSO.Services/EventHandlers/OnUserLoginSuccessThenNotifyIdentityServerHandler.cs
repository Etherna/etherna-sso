using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using IdentityServer4.Services;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnUserLoginSuccessThenNotifyIdentityServerHandler : EventHandlerBase<UserLoginSuccessEvent>
    {
        // Fields.
        private readonly IEventService identityServerEventService;

        // Constructor.
        public OnUserLoginSuccessThenNotifyIdentityServerHandler(
            IEventService identityServerEventService)
        {
            this.identityServerEventService = identityServerEventService;
        }

        // Methods.
        public override async Task HandleAsync(UserLoginSuccessEvent @event)
        {
            await identityServerEventService.RaiseAsync(
                @event.Provider is null ?
                new IdentityServer4.Events.UserLoginSuccessEvent(
                    @event.User.Username,
                    @event.User.Id,
                    @event.User.Username,
                    clientId: @event.ClientId) :
                new IdentityServer4.Events.UserLoginSuccessEvent(
                    @event.Provider,
                    @event.ProviderUserId,
                    @event.User.Id,
                    @event.User.Username,
                    clientId: @event.ClientId));
        }
    }
}
