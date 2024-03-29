﻿// Copyright 2021-present Etherna Sa
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
    public class UserDto
    {
        // Constructor.
        public UserDto(UserBase user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

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
