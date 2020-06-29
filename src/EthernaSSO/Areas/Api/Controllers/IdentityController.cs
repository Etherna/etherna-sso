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
