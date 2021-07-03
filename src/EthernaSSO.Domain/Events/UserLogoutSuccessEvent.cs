using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Models;
using System;

namespace Etherna.SSOServer.Domain.Events
{
    public class UserLogoutSuccessEvent : IDomainEvent
    {
        public UserLogoutSuccessEvent(User user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public User User { get; }
    }
}
