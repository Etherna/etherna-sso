using Etherna.SSOServer.Areas.Api.DtoModels;
using Etherna.SSOServer.Attributes;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.2")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class IdentityController : Controller
    {
        // Fields.
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        // Constructors.
        public IdentityController(
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        // Methods.
        /// <summary>
        /// Get information about current logged in user.
        /// </summary>
        /// <response code="200">Current user information</response>
        [HttpGet]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<UserDto> GetCurrentUserAsync()
        {
            if (!signInManager.IsSignedIn(User))
                throw new UnauthorizedAccessException();

            var user = await userManager.GetUserAsync(User);
            return new UserDto(user);
        }
    }
}
