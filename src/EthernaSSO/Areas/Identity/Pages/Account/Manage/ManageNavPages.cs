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
