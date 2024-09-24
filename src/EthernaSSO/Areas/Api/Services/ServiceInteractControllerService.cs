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
