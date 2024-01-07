// Copyright 2021-present Etherna Sa
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
using Etherna.SSOServer.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.3")]
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
