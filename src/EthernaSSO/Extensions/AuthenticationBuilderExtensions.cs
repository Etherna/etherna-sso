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


using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etherna.SSOServer.Extensions
{
    internal static class AuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds a jwt bearer scheme accepting the access tokens that the SSO itself issues for an audience.
        /// </summary>
        public static AuthenticationBuilder AddSsoJwtBearer(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            string audience,
            string authority,
            bool requireHttpsMetadata)
        {
            ArgumentNullException.ThrowIfNull(builder);

            return builder.AddJwtBearer(authenticationScheme, options =>
            {
                options.Audience = audience;
                options.Authority = authority;
                //keep the jwt claim types: Identity looks the user id up as "sub", as IdentityServer configures it,
                //while the default mapping renames it to ClaimTypes.NameIdentifier
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = requireHttpsMetadata;
            });
        }
    }
}
