using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Etherna.SSOServer.Domain.Helpers
{
    public static class UsernameHelper
    {
        // Consts.
        public const string AllowedUsernameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        public const string UsernameRegex = "^[a-zA-Z0-9_]{5,20}$";

        // Methods.
        public static bool IsValidUsername(string username) =>
            Regex.IsMatch(username, UsernameRegex);

        public static string NormalizeUsername(string username)
        {
            if (username is null)
                throw new ArgumentNullException(nameof(username));

            username = username.ToUpper(CultureInfo.InvariantCulture); //to upper case

            return username;
        }
    }
}
