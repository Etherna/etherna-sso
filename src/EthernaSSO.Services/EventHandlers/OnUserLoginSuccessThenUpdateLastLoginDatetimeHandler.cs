using Etherna.DomainEvents;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Events;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnUserLoginSuccessThenUpdateLastLoginDateTimeHandler : EventHandlerBase<UserLoginSuccessEvent>
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructors.
        public OnUserLoginSuccessThenUpdateLastLoginDateTimeHandler(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Methods.
        public override async Task HandleAsync(UserLoginSuccessEvent @event)
        {
            @event.User.UpdateLastLoginDateTime();
            await ssoDbContext.SaveChangesAsync();
        }
    }
}
