using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Domain.Events
{
    public class UserLogoutSuccessEvent : IDomainEvent
    {
        public UserLogoutSuccessEvent(User? user)
        {
            User = user;
        }

        public User? User { get; } //nullable because user may have been removed
    }
}
