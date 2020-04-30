using Digicando.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    public class OnCreatedUserThenLoginHandler : EventHandlerBase<EntityCreatedEvent<User>>
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
        public override Task HandleAsync(EntityCreatedEvent<User> @event) =>
             signInManager.SignInAsync(@event.Entity, true);
    }
}
