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
using Etherna.SSOServer.Configs;
using Etherna.SwarmSdk.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;

namespace Etherna.SSOServer.Areas.Api
{
    public static class SsoApiMapper
    {
        // Methods.
        public static void MapSsoApi(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            // APIs.
            ConfigureV03Maps(app.MapGroup("/api/v0.3").WithMetadata(new SsoApiMarker()));
        }

        // Helpers.
        private static void ConfigureV03Maps(RouteGroupBuilder builder)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            //identity
            builder.MapGet("identity",
                    (ISsoApiHandler handler) =>
                        handler.GetCurrentUserPrivateInfoAsync())
                .RequireAuthorization(CommonConsts.UserInteractApiScopePolicy)
                .Produces<PrivateUserDto>();

            builder.MapGet("identity/address/{etherAddress}",
                    (ISsoApiHandler handler,
                            [FromRoute] EthAddress etherAddress) =>
                        handler.GetUserByEtherAddressAsync(etherAddress))
                .AllowAnonymous()
                .Produces<UserDto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            builder.MapGet("identity/email/{email}",
                    (ISsoApiHandler handler,
                            [FromRoute] string email) =>
                        handler.IsEmailRegisteredAsync(email))
                .AllowAnonymous()
                .Produces<bool>()
                .Produces(StatusCodes.Status400BadRequest);

            builder.MapGet("identity/username/{username}",
                    (ISsoApiHandler handler,
                            [FromRoute] string username) =>
                        handler.GetUserByUsernameAsync(username))
                .AllowAnonymous()
                .Produces<UserDto>()
                .Produces(StatusCodes.Status404NotFound);

            //serviceInteract
            builder.MapGet("serviceInteract/contacts/{etherAddress}",
                    (ISsoApiHandler handler,
                            [FromRoute] EthAddress etherAddress) =>
                        handler.GetUserContactInfoAsync(etherAddress))
                .RequireAuthorization(CommonConsts.ServiceInteractApiScopePolicy)
                .Produces<UserContactInfoDto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}