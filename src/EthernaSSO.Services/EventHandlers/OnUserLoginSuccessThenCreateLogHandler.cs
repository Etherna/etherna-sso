using Etherna.DomainEvents;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models.Logs;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnUserLoginSuccessThenCreateLogHandler : EventHandlerBase<UserLoginSuccessEvent>
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructors.
        public OnUserLoginSuccessThenCreateLogHandler(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext ?? throw new ArgumentNullException(nameof(ssoDbContext));
        }

        // Methods.
        public override async Task HandleAsync(UserLoginSuccessEvent @event)
        {
            var log = new UserLoggedInLog(@event.User);
            await ssoDbContext.Logs.CreateAsync(log);
        }
    }
}
