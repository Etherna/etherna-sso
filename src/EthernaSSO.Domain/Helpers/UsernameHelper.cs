﻿// Copyright 2021-present Etherna Sa
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
