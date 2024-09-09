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
    public class ServiceInteractController(IServiceInteractControllerService service) : ControllerBase
    {
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
            service.GetUserContactInfoAsync(etherAddress);
    }
}
