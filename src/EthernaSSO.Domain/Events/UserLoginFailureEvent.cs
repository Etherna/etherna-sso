using Etherna.DomainEvents;
using System;

namespace Etherna.SSOServer.Domain.Events
{
    public class UserLoginFailureEvent : IDomainEvent
    {
        public UserLoginFailureEvent(string identifier, string error, string? clientId)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException($"'{nameof(error)}' cannot be null or whitespace.", nameof(error));

            ClientId = clientId;
            Error = error;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public string? ClientId { get; }
        public string Error { get; }
        public string Identifier { get; }
    }
}
