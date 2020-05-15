using Etherna.SSOServer.Domain.Models;
using System;

namespace Etherna.SSOServer.Areas.Api.DtoModels
{
    public class UserDto
    {
        // Constructor.
        public UserDto(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            Email = user.Email;
            EtherAddress = user.EtherAddress;
            EtherManagedPrivateKey = user.EtherManagedPrivateKey;
            EtherLoginAddress = user.EtherLoginAddress;
            PhoneNumber = user.PhoneNumber;
            Username = user.Username;
        }

        // Properties.
        public virtual string? Email { get; }
        public virtual string EtherAddress { get; }
        public virtual string? EtherManagedPrivateKey { get; }
        public virtual string? EtherLoginAddress { get; }
        public virtual string? PhoneNumber { get; }
        public virtual string? Username { get; }
    }
}
