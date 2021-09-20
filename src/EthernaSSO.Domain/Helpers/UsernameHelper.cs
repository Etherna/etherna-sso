using System;
using System.Globalization;

namespace Etherna.SSOServer.Domain.Helpers
{
    public static class UsernameHelper
    {
        public static string NormalizeUsername(string username)
        {
            if (username is null)
                throw new ArgumentNullException(nameof(username));

            username = username.ToUpper(CultureInfo.InvariantCulture); //to upper case

            return username;
        }
    }
}
