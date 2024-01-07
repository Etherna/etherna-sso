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

using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public static class ManageNavPages
    {
        // Properties.
        public static string Index => "Index";
        public static string ApiKeys => "ApiKeys";
        public static string Email => "Email";
        public static string ChangePassword => "ChangePassword";
        public static string DownloadPersonalData => "DownloadPersonalData";
        public static string DeletePersonalData => "DeletePersonalData";
        public static string Web3Login => "Web3Login";
        public static string PersonalData => "PersonalData";
        public static string TwoFactorAuthentication => "TwoFactorAuthentication";

        // Methods.
        public static string? IndexNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, Index);
        }

        public static string? ApiKeysNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, ApiKeys);
        }

        public static string? EmailNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, Email);
        }

        public static string? ChangePasswordNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, ChangePassword);
        }

        public static string? DeletePersonalDataNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, DeletePersonalData);
        }

        public static string? DownloadPersonalDataNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, DownloadPersonalData);
        }

        public static string? Web3LoginNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, Web3Login);
        }

        public static string? PersonalDataNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, PersonalData);
        }

        public static string? TwoFactorAuthenticationNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, TwoFactorAuthentication);
        }

        // Helpers.
        private static string? PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
