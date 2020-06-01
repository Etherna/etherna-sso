using Etherna.SSOServer.Areas.Api.DtoModels;
using Etherna.SSOServer.Attributes;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> userManager;

        // Constructors.
        public IdentityController(
            UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        // Methods.
        /// <summary>
        /// Get information about current logged in user.
        /// </summary>
        /// <response code="200">Current user information</response>
        [HttpGet]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<PrivateUserDto> GetCurrentUserAsync()
        {
            var user = await userManager.GetUserAsync(User);
            return new PrivateUserDto(user);
        }
    }
}
