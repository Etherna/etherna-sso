using System.Text.RegularExpressions;

namespace Etherna.SSOServer.Domain.Helpers
{
    public static class EmailHelper
    {
        // Consts.
        public const string EmailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

        // Static methods.
        public static bool IsValidEmail(string email) =>
            Regex.IsMatch(email, EmailRegex, RegexOptions.IgnoreCase);
    }
}
