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

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;

namespace Etherna.SSOServer.Extensions
{
    internal static class HttpRequestExtensions
    {
        /// <summary>
        /// Tells if the request authenticates with a bearer token, as API clients do, instead of the site cookie.
        /// </summary>
        public static bool HasBearerToken(this HttpRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string? authorization = request.Headers[HeaderNames.Authorization];
            return !string.IsNullOrEmpty(authorization) &&
                authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
        }
    }
}
