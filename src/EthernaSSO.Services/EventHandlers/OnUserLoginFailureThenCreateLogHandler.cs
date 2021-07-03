using Etherna.DomainEvents;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models.Logs;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnUserLoginFailureThenCreateLogHandler : EventHandlerBase<UserLoginFailureEvent>
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructors.
        public OnUserLoginFailureThenCreateLogHandler(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Methods.
        public override async Task HandleAsync(UserLoginFailureEvent @event)
        {
            var log = new UserLoginFailureLog(@event.Error, @event.Identifier);
            await ssoDbContext.Logs.CreateAsync(log);
        }
    }
}
