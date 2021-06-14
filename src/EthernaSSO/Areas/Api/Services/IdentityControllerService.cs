﻿//   Copyright 2021-present Etherna Sagl
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
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Nethereum.Util;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Services
{
    public class IdentityControllerService : IIdentityControllerService
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<User> userManager;

        // Constructors.
        public IdentityControllerService(
            ISsoDbContext ssoDbContext,
            UserManager<User> userManager)
        {
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Methods.
        public async Task<PrivateUserDto> GetPrivateUserInfoByClaimsAsync(ClaimsPrincipal userClaims)
        {
            var user = await userManager.GetUserAsync(userClaims);
            return new PrivateUserDto(user);
        }

        public async Task<UserDto> GetUserByEtherAddressAsync(string etherAddress)
        {
            if (!etherAddress.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("Invalid address", nameof(etherAddress));

            etherAddress = etherAddress.ConvertToEthereumChecksumAddress();

            var user = await ssoDbContext.Users.FindOneAsync(
                u => u.EtherAddress == etherAddress ||
                u.EtherPreviousAddresses.Contains(etherAddress));
            return new UserDto(user);
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            username = User.NormalizeUsername(username);

            var user = await ssoDbContext.Users.FindOneAsync(u => u.NormalizedUsername == username);
            return new UserDto(user);
        }
    }
}
