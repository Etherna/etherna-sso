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

using Etherna.SSOServer.Domain.Models.ClientAppAgg;
using System.Collections.Generic;

namespace Etherna.SSOServer.Persistence.Settings
{
    /// <summary>
    /// A client app created at db seeding, owned by the first admin user (see "DbSeed:Clients").
    /// Used to initialize development environments with the same clients that production
    /// environments define from the developer editor.
    /// </summary>
    public class SsoDbSeedClientDefinition
    {
        // Properties.
        public IEnumerable<string> AllowedScopes { get; set; } = [];
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public ClientAppType ClientType { get; set; }
        public string Secret { get; set; } = null!;
    }
}
