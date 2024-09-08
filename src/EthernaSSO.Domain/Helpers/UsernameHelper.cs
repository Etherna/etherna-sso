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
using System.Globalization;
using System.Text.RegularExpressions;

namespace Etherna.SSOServer.Domain.Helpers
{
    public static partial class UsernameHelper
    {
        // Consts.
        public const string AllowedUsernameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        public const string UsernameRegex = "^[a-zA-Z0-9_]{5,25}$";
        public const string UsernameValidationErrorMessage = "Allowed characters are a-z, A-Z, 0-9, _. Permitted length is between 5 and 25.";

        // Methods.
        public static bool IsValidUsername(string username) =>
            UsernameRegexHelper().IsMatch(username);

        public static string NormalizeUsername(string username)
        {
            ArgumentNullException.ThrowIfNull(username, nameof(username));

            username = username.ToUpper(CultureInfo.InvariantCulture); //to upper case

            return username;
        }

        // Helpers.
        [GeneratedRegex(UsernameRegex)]
        private static partial Regex UsernameRegexHelper();
    }
}
