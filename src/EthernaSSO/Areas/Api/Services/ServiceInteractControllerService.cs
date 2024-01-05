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

using Etherna.SSOServer.Areas.Api.DtoModels;
using Etherna.SSOServer.Services.Domain;
using Nethereum.Util;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Services
{
    public class ServiceInteractControllerService : IServiceInteractControllerService
    {
        // Fields.
        private readonly IUserService userService;

        // Constructor.
        public ServiceInteractControllerService(IUserService userService)
        {
            this.userService = userService;
        }

        // Methods.
        public async Task<UserContactInfoDto> GetUserContactInfoAsync(string etherAddress)
        {
            if (!etherAddress.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("Invalid address", nameof(etherAddress));

            var user = await userService.FindUserByAddressAsync(etherAddress);
            return new UserContactInfoDto(user);
        }
    }
}
