// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.SSOServer.Domain.Models;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Areas.Api.DtoModels
{
    public class PrivateUserDto
    {
        // Constructor.
        public PrivateUserDto(UserBase user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            Email = user.Email;
            EtherAddress = user.EtherAddress;
            EtherPreviousAddresses = user.EtherPreviousAddresses;
            PhoneNumber = user.PhoneNumber;
            Username = user.Username;

            switch (user)
            {
                case UserWeb2 userWeb2:
                    AccountType = "web2";
                    EtherManagedPrivateKey = userWeb2.EtherManagedPrivateKey;
                    EtherLoginAddress = userWeb2.EtherLoginAddress;
                    break;
                case UserWeb3 _:
                    AccountType = "web3";
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        // Properties.
        public string AccountType { get; }
        public string? Email { get; }
        public string EtherAddress { get; }
        public string? EtherManagedPrivateKey { get; }
        public IEnumerable<string> EtherPreviousAddresses { get; }
        public string? EtherLoginAddress { get; }
        public string? PhoneNumber { get; }
        public string? Username { get; }
    }
}
