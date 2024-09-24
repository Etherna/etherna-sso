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

using Etherna.ACR.Helpers;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Areas.Api.DtoModels;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Identity;
using Nethereum.Util;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Services
{
    public class IdentityControllerService(
        ISsoDbContext context,
        UserManager<UserBase> userManager,
        IUserService userService)
        : IIdentityControllerService
    {
        // Methods.
        public async Task<UserDto> GetUserByEtherAddressAsync(string etherAddress)
        {
            if (!etherAddress.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("Invalid address", nameof(etherAddress));

            var user = await userService.FindUserByAddressAsync(etherAddress);
            return new UserDto(user);
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            username = UsernameHelper.NormalizeUsername(username);

            var user = await context.Users.FindOneAsync(u => u.NormalizedUsername == username);
            return new UserDto(user);
        }

        public Task<bool> IsEmailRegisteredAsync(string email) =>
            context.Users.QueryElementsAsync(users => users.AnyAsync(u => u.NormalizedEmail == EmailHelper.NormalizeEmail(email)));

        public async Task<PrivateUserDto?> TryGetPrivateUserInfoByClaimsAsync(ClaimsPrincipal userClaims)
        {
            var user = await userManager.GetUserAsync(userClaims);
            return user is null ? null : new PrivateUserDto(user);
        }
    }
}
