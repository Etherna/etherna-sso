using Digicando.DomainEvents;
using Etherna.SSOServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Etherna.SSOServer.Domain.Events
{
    public class UserLoggedOutEvent : IDomainEvent
    {
        public UserLoggedOutEvent(User user)
        {
            User = user;
        }

        public User User { get; }
    }
}
