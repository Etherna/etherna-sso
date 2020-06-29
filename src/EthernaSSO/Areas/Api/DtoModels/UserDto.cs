using Etherna.SSOServer.Domain.Models;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Areas.Api.DtoModels
{
    public class UserDto
    {
        // Constructor.
        public UserDto(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            EtherAddress = user.EtherAddress;
            EtherPreviousAddresses = user.EtherPreviousAddresses;
            Username = user.Username;
        }

        // Properties.
        public string EtherAddress { get; }
        public IEnumerable<string> EtherPreviousAddresses { get; }
        public string? Username { get; }
    }
}
