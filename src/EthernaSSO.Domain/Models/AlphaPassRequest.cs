using Etherna.ACR.Helpers;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Etherna.SSOServer.Domain.Models
{
    public class AlphaPassRequest : EntityModelBase<string>
    {
        // Consts.
        public const int CodeLength = 10;
        public readonly static string CodeValidChars = "0123456789" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Constructor.
        public AlphaPassRequest(string email)
        {
            if (!EmailHelper.IsValidEmail(email))
                throw new ArgumentException("Email is not valid", nameof(email));

            IsEmailConfirmed = false;
            NormalizedEmail = EmailHelper.NormalizeEmail(email);
            Secret = GenerateNewCode();
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected AlphaPassRequest() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual bool IsEmailConfirmed { get; protected set; }
        public virtual bool IsInvitationSent { get; set; }
        public virtual string NormalizedEmail { get; protected set; }
        public virtual string Secret { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(IsEmailConfirmed))]
        public bool ConfirmEmail(string secret)
        {
            if (Secret != secret)
                return false;
            IsEmailConfirmed = true;
            return true;
        }

        // Helpers.
        [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "A different code each login is sufficient. Secure randomness is not required")]
        private string GenerateNewCode()
        {
            var length = CodeLength;
            var dictionary = CodeValidChars;

            var codeBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
                codeBuilder.Append(dictionary[Random.Shared.Next(dictionary.Length)]);

            return codeBuilder.ToString();
        }
    }
}
