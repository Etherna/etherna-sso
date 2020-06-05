using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnCreatedUserThenLoginHandler : EventHandlerBase<EntityCreatedEvent<User>>
    {
        // Fields.
        private readonly SignInManager<User> signInManager;

        // Constructors.
        public OnCreatedUserThenLoginHandler(
            SignInManager<User> signInManager)
        {
            this.signInManager = signInManager;
        }

        // Methods.
        public override Task HandleAsync(EntityCreatedEvent<User> @event)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));

            return signInManager.SignInAsync(@event.Entity, false);
        }
    }
}
