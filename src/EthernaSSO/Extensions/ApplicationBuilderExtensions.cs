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
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Etherna.SSOServer.Extensions
{
    internal static class ApplicationBuilderExtensions
    {
        // Consts.
        //the API and the IdentityServer protocol endpoints
        private static readonly string[] NonPagePathPrefixes = ["/api", "/connect", "/.well-known"];

        // Methods.
        /// <summary>
        /// Re-executes the status code page only for the requests of a browser navigating the site.
        /// API calls, IdentityServer protocol endpoints and bearer authenticated requests keep their bare
        /// status code. The page layout resolves the roles of the user, which for a bearer principal is a
        /// userinfo request to this same server: an error of that request rendered as a page would repeat
        /// the lookup with the same token, and so on without end.
        /// </summary>
        public static IApplicationBuilder UseStatusCodePagesOnPageRequests(
            this IApplicationBuilder app,
            string pathFormat,
            string queryFormat)
        {
            ArgumentNullException.ThrowIfNull(app);

            return app.UseWhen(
                context => !context.Request.HasBearerToken() &&
                           !NonPagePathPrefixes.Any(prefix =>
                               context.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)),
                branch => branch.UseStatusCodePagesWithReExecute(pathFormat, queryFormat));
        }
    }
}
