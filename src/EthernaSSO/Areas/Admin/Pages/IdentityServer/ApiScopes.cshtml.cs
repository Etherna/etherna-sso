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

using Duende.IdentityServer.Models;
using Etherna.SSOServer.Configs.IdentityServer;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class ApiScopesModel(IdServerConfig idServerConfig) : PageModel
    {
        // Properties.
        public List<ApiScope> ApiScopes { get; private set; } = new();

        // Methods.
        public void OnGet()
        {
            ApiScopes = idServerConfig.ApiScopes.ToList();
        }
    }
}
