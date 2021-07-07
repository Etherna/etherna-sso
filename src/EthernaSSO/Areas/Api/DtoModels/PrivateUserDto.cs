//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
            if (user is null)
                throw new ArgumentNullException(nameof(user));

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
