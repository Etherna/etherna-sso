// Copyright 2021-present Etherna Sa
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
