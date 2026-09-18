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

using System.Diagnostics.CodeAnalysis;

namespace Etherna.SSOServer.Services.Options
{
    public sealed class MatomoOptions
    {
        // Properties.
        /// <summary>
        /// The tracking is active only when both the site id and the tracker url are configured.
        /// </summary>
        [MemberNotNullWhen(true, nameof(SiteId), nameof(TrackerUrl))]
        public bool IsEnabled => !string.IsNullOrEmpty(SiteId) && !string.IsNullOrEmpty(TrackerUrl);

        /// <summary>
        /// True when only one of the two values is configured: the tracking would stay off unnoticed.
        /// </summary>
        public bool IsPartiallyConfigured => string.IsNullOrEmpty(SiteId) != string.IsNullOrEmpty(TrackerUrl);

        public string? SiteId { get; set; }

        /// <summary>
        /// Base url of the Matomo instance, hosting <c>matomo.php</c> and <c>matomo.js</c>.
        /// </summary>
        [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Bound from configuration and rendered as is")]
        public string? TrackerUrl { get; set; }
    }
}
