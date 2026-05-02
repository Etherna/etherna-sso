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

using Microsoft.AspNetCore.Builder;
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
            
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}