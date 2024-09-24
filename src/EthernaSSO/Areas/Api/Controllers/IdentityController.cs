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
using Etherna.SSOServer.Areas.Api.Services;
using Etherna.SSOServer.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.3")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class IdentityController(IIdentityControllerService service) : Controller
    {
        // GET.

        /// <summary>
        /// Get private information about current logged in user.
        /// </summary>
        /// <response code="200">Current user information</response>
        [HttpGet]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<PrivateUserDto> GetCurrentUserAsync() =>
            await service.TryGetPrivateUserInfoByClaimsAsync(User) ?? throw new InvalidOperationException();

        /// <summary>
        /// Get information about user by its ethereum address.
        /// </summary>
        /// <param name="etherAddress">User's ethereum address</param>
        /// <response code="200">User information</response>
        [HttpGet("address/{etherAddress}")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<UserDto> GetUserByEtherAddressAsync(string etherAddress) =>
            service.GetUserByEtherAddressAsync(etherAddress);

        /// <summary>
        /// Verify if an email is registered.
        /// </summary>
        /// <param name="email">User's email</param>
        /// <response code="200">True if email is registered, false otherwise</response>
        [HttpGet("email/{email}")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<bool> IsEmailRegistered(string email) =>
            service.IsEmailRegisteredAsync(email);

        /// <summary>
        /// Get information about user by its username.
        /// </summary>
        /// <param name="username">User's username</param>
        /// <response code="200">User information</response>
        [HttpGet("username/{username}")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<UserDto> GetUserByUsernameAsync(string username) =>
            service.GetUserByUsernameAsync(username);
    }
}
