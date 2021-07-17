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

using Etherna.SSOServer.Areas.Api.DtoModels;
using Etherna.SSOServer.Areas.Api.Services;
using Etherna.SSOServer.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.2")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class IdentityController : Controller
    {
        // Fields.
        private readonly IIdentityControllerService identityControllerService;

        // Constructors.
        public IdentityController(
            IIdentityControllerService identityControllerService)
        {
            this.identityControllerService = identityControllerService;
        }

        // Methods.
        /// <summary>
        /// Get private information about current logged in user.
        /// </summary>
        /// <response code="200">Current user information</response>
        [HttpGet]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<PrivateUserDto> GetCurrentUserAsync() =>
            identityControllerService.GetPrivateUserInfoByClaimsAsync(User);

        /// <summary>
        /// Get information about user by its ethereum address.
        /// </summary>
        /// <param name="etherAddress">User's ethereum address</param>
        /// <response code="200">User information</response>
        [HttpGet("address/{etherAddress}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<UserDto> GetUserByEtherAddressAsync(string etherAddress) =>
            identityControllerService.GetUserByEtherAddressAsync(etherAddress);

        /// <summary>
        /// Get contact information about an user.
        /// </summary>
        /// <param name="etherAddress">User's ethereum address</param>
        /// <response code="200">User contact information</response>
        [HttpGet("address/{etherAddress}/contact")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<UserContactInfoDto> GetUserContactInfoAsync(string etherAddress) =>
            identityControllerService.GetUserContactInfoAsync(etherAddress);

        /// <summary>
        /// Verify if an email is registered.
        /// </summary>
        /// <param name="email">User's email</param>
        /// <response code="200">True if email is registered, false otherwise</response>
        [HttpGet("email/{email}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<bool> IsEmailRegistered(string email) =>
            identityControllerService.IsEmailRegisteredAsync(email);

        /// <summary>
        /// Get information about user by its username.
        /// </summary>
        /// <param name="username">User's username</param>
        /// <response code="200">User information</response>
        [HttpGet("username/{username}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<UserDto> GetUserByUsernameAsync(string username) =>
            identityControllerService.GetUserByUsernameAsync(username);
    }
}
