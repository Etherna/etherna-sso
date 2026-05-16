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

namespace Etherna.SSOServer.Domain.Models.ClientAppAgg
{
    public enum ClientAppType
    {
        /// <summary>
        /// Confidential server-side web application (Authorization Code flow).
        /// </summary>
        WebApp,

        /// <summary>
        /// Public native application - desktop, mobile, or SPA (Authorization Code + PKCE).
        /// </summary>
        NativeApp,

        /// <summary>
        /// Machine-to-machine backend service (Client Credentials flow).
        /// </summary>
        ClientCredential,

        /// <summary>
        /// Custom client created by admin without a predefined template.
        /// </summary>
        Custom
    }
}
