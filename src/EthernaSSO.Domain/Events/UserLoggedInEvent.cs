using Digicando.DomainEvents;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Domain.Events
{
    public class UserLoggedInEvent : IDomainEvent
    {
        // Enums.
        public enum LoginMethod
        {
            ExternalProvider,
            Password,
            Web3
        }

        // Constructors.
        public UserLoggedInEvent(
            User user,
            LoginMethod method,
            string? additionalData = null)
        {
            AdditionalData = additionalData;
            Method = method;
            User = user;
        }

        // Properties.
        public string? AdditionalData { get; }
        public LoginMethod Method { get; }
        public User User { get; }
    }
}
