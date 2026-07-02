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

using System.Collections.Generic;

namespace Etherna.SSOServer.Configs.IdentityServer
{
    /// <summary>
    /// An IdentityServer client defined entirely by configuration (see "IdServer:ConfigClients").
    /// Used to host environment-specific clients in configuration files, instead of hardcoding
    /// them in <see cref="IdServerConfig"/>.
    /// </summary>
    public class ConfigClientDefinition
    {
        // Properties.
        public IEnumerable<string> AllowedGrantTypes { get; set; } = [];
        public IEnumerable<string> AllowedScopes { get; set; } = [];
        public bool AllowOfflineAccess { get; set; }
        public string ClientId { get; set; } = null!;
        public string? ClientName { get; set; }
        public IEnumerable<string> PostLogoutRedirectUris { get; set; } = [];
        public IEnumerable<string> RedirectUris { get; set; } = [];
        public string? Secret { get; set; }
    }
}
