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

namespace Etherna.SSOServer.Configs
{
    public static class CommonConsts
    {
        public const string AdminArea = "Admin";
        public const string AlphaPassArea = "AlphaPass";
        public const string ApiArea = "Api";
        public const string IdentityArea = "Identity";

        public const string DatabaseAdminPath = "/admin/db";
        public const string HangfireAdminPath = "/admin/hangfire";

        public const string RequireAdministratorRolePolicy = "RequireAdministratorRolePolicy";
        public const string ServiceInteractApiScopePolicy = "ServiceInteractApiScopePolicy";
        public const string UserInteractApiScopePolicy = "UserInteractApiScopePolicy";

        public const string UserAuthenticationPolicyScheme = "userAuthnPolicyScheme";
        public const string UserAuthenticationJwtScheme = "userAuthnJwtScheme";
        public const string ServiceAuthenticationScheme = "serviceAuthnScheme";
    }
}
