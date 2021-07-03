using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Models;
using System;

namespace Etherna.SSOServer.Domain.Events
{
    public class UserLoginSuccessEvent : IDomainEvent
    {
        public UserLoginSuccessEvent(User user, string? clientId = null, string? provider = null, string? providerUserId = null)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            ClientId = clientId;
            Provider = provider;
            ProviderUserId = providerUserId;
        }

        public string? ClientId { get; }
        public string? Provider { get; }
        public string? ProviderUserId { get; }
        public User User { get; }
    }
}
