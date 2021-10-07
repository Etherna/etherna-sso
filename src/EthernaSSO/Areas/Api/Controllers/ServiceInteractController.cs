using Etherna.SSOServer.Areas.Api.DtoModels;
using Etherna.SSOServer.Areas.Api.Services;
using Etherna.SSOServer.Attributes;
using Etherna.SSOServer.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.2")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [Authorize(CommonConsts.ServiceInteractApiScopePolicy)]
    public class ServiceInteractController : ControllerBase
    {
        // Fields.
        private readonly IServiceInteractControllerService controllerService;

        // Constructor.
        public ServiceInteractController(IServiceInteractControllerService controllerService)
        {
            this.controllerService = controllerService;
        }

        /// <summary>
        /// Get contact information about an user.
        /// </summary>
        /// <param name="etherAddress">User's ethereum address</param>
        /// <response code="200">User contact information</response>
        [HttpGet("contacts/{etherAddress}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<UserContactInfoDto> GetUserContactInfoAsync(string etherAddress) =>
            controllerService.GetUserContactInfoAsync(etherAddress);
    }
}
