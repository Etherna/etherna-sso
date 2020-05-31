using Etherna.SSOServer.Domain.Models;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Areas.Api.DtoModels
{
    public class PrivateUserDto
    {
        // Constructor.
        public PrivateUserDto(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            Email = user.Email;
            EtherAddress = user.EtherAddress;
            EtherManagedPrivateKey = user.EtherManagedPrivateKey;
            EtherPreviousAddresses = user.EtherPreviousAddresses;
            EtherLoginAddress = user.EtherLoginAddress;
            PhoneNumber = user.PhoneNumber;
            Username = user.Username;
        }

        // Properties.
        public string? Email { get; }
        public string EtherAddress { get; }
        public string? EtherManagedPrivateKey { get; }
        public IEnumerable<string> EtherPreviousAddresses { get; }
        public string? EtherLoginAddress { get; }
        public string? PhoneNumber { get; }
        public string? Username { get; }
    }
}
