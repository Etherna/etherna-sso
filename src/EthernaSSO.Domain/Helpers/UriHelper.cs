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

using System;
using System.Net;

namespace Etherna.SSOServer.Domain.Helpers
{
    public static class UriHelper
    {
        /// <summary>
        /// Validates an OAuth redirect URI.
        /// Web apps accept only https:// or http://localhost.
        /// Native apps additionally accept custom URI schemes (e.g. myapp://callback).
        /// Raw IP addresses (other than localhost/127.0.0.1) are rejected.
        /// </summary>
        public static bool IsValidRedirectUri(string uri, bool allowCustomSchemes = false)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return false;

            if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
                return false;

            if (parsed.Scheme == Uri.UriSchemeHttps)
                return IsValidHost(parsed.Host);

            if (parsed.Scheme == Uri.UriSchemeHttp)
                return IsLocalHost(parsed.Host);

            // Custom schemes are allowed for native apps (e.g. myapp://callback).
            return allowCustomSchemes;
        }

        /// <summary>
        /// Validates an OAuth CORS origin.
        /// Must be scheme://host[:port] with no path, query, or fragment.
        /// Accepts https:// or http://localhost. Raw IP addresses are rejected.
        /// </summary>
        public static bool IsValidCorsOrigin(string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return false;

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var parsed))
                return false;

            // Origin must not carry a path, query or fragment beyond the root.
            if (parsed.AbsolutePath is not ("" or "/"))
                return false;
            if (!string.IsNullOrEmpty(parsed.Query))
                return false;
            if (!string.IsNullOrEmpty(parsed.Fragment))
                return false;

            if (parsed.Scheme == Uri.UriSchemeHttps)
                return IsValidHost(parsed.Host);

            if (parsed.Scheme == Uri.UriSchemeHttp)
                return IsLocalHost(parsed.Host);

            return false;
        }

        // Helpers.
        private static bool IsLocalHost(string host) =>
            string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
            host == "127.0.0.1" ||
            host == "::1";

        private static bool IsValidHost(string host)
        {
            if (string.IsNullOrEmpty(host))
                return false;

            // Reject raw IP addresses; only localhost variants are allowed.
            if (IPAddress.TryParse(host, out _))
                return IsLocalHost(host);

            // Require at least one dot so bare single-label names are rejected.
            return host.Contains('.', StringComparison.Ordinal);
        }
    }
}
