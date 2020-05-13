using Digicando.DomainHelper.Attributes;
using Etherna.SSOServer.Domain.Models.UserAgg;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etherna.SSOServer.Domain.Models
{
    public class User : EntityModelBase<string>
    {
        // Consts.
        public const string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        public const string EmailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
        public const string UsernameRegex = "^[a-zA-Z0-9]+(?:[._-]?[a-zA-Z0-9])*$";

        // Fields.
        private List<UserLoginInfo> _logins = new List<UserLoginInfo>();
        private List<string> _twoFactorRecoveryCode = new List<string>();

        // Constructors and dispose.
        ///// <summary>
        ///// Web3 unmanaged user registration
        ///// </summary>
        ///// <param name="address">The user public address</param>
        //public User(string address)
        //{
        //    Address = address;
        //}

        /// <summary>
        /// Web2 managed user registration
        /// </summary>
        /// <param name="account">The user's account, managed by SSO Server</param>
        /// <param name="email">Optional user email</param>
        /// <param name="username">Optional username</param>
        /// <param name="web3LoginAddress">Optional address of account managed by user. Valid for authentication</param>
        public User(
            EtherAccount account,
            string? email = default,
            string? username = default,
            string? web3LoginAddress = default)
        {
            EtherAccount = account;
            if (email != null) SetEmail(email);
            if (username != null) SetUsername(username);
            if (web3LoginAddress != null) LoginAccount = new EtherAccount(web3LoginAddress);
        }
        protected User() { }

        // Static builders.


        // Properties.
        public virtual int AccessFailedCount { get; internal protected set; }
        public virtual string? AuthenticatorKey { get; internal protected set; }
        public virtual string? Email { get; protected set; }
        public virtual bool EmailConfirmed { get; protected set; }
        public virtual EtherAccount EtherAccount { get; protected set; } = default!;
        public virtual bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
        public virtual bool LockoutEnabled { get; internal protected set; }
        public virtual DateTimeOffset? LockoutEnd { get; internal protected set; }
        public virtual EtherAccount? LoginAccount { get; protected set; }
        public virtual IEnumerable<UserLoginInfo> Logins
        {
            get => _logins;
            protected set => _logins = new List<UserLoginInfo>(value ?? Array.Empty<UserLoginInfo>());
        }
        public virtual string? NormalizedEmail { get; protected set; }
        public virtual string? NormalizedUsername { get; protected set; }
        public virtual string? PasswordHash { get; internal protected set; }
        public virtual string? PhoneNumber { get; protected set; }
        public virtual bool PhoneNumberConfirmed { get; protected set; }
        public virtual string SecurityStamp { get; internal protected set; } = default!;
        public virtual bool TwoFactorEnabled { get; internal protected set; }
        public virtual IEnumerable<string> TwoFactorRecoveryCodes
        {
            get => _twoFactorRecoveryCode;
            internal protected set => _twoFactorRecoveryCode = new List<string>(value ?? Array.Empty<string>());
        }
        public virtual string? Username { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(Logins))]
        public virtual bool AddLogin(UserLoginInfo userLoginInfo)
        {
            //avoid multiaccounting with same provider
            if (Logins.Any(login => login.LoginProvider == userLoginInfo.ProviderKey))
                return false;

            _logins.Add(userLoginInfo);
            return true;
        }

        [PropertyAlterer(nameof(EmailConfirmed))]
        public virtual void ConfirmEmail() =>
            EmailConfirmed = true;

        [PropertyAlterer(nameof(PhoneNumberConfirmed))]
        public virtual void ConfirmPhoneNumber() =>
            PhoneNumberConfirmed = true;

        [PropertyAlterer(nameof(TwoFactorRecoveryCodes))]
        public virtual bool RedeemTwoFactorRecoveryCode(string code)
        {
            if (_twoFactorRecoveryCode.Contains(code))
            {
                _twoFactorRecoveryCode.Remove(code);
                return true;
            }
            return false;
        }

        [PropertyAlterer(nameof(Logins))]
        public virtual void RemoveLogin(string loginProvider, string providerKey) =>
            _logins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);

        [PropertyAlterer(nameof(Email))]
        [PropertyAlterer(nameof(EmailConfirmed))]
        [PropertyAlterer(nameof(NormalizedEmail))]
        public virtual void SetEmail(string email)
        {
            if (!Regex.IsMatch(email, EmailRegex, RegexOptions.IgnoreCase))
                throw new ArgumentOutOfRangeException(nameof(email));

            if (Email != email)
            {
                Email = email;
                EmailConfirmed = false;
                NormalizedEmail = NormalizeEmail(email);
            }
        }

        [PropertyAlterer(nameof(PhoneNumber))]
        [PropertyAlterer(nameof(PhoneNumberConfirmed))]
        public virtual void SetPhoneNumber(string phoneNumber)
        {
            if (PhoneNumber != phoneNumber)
            {
                PhoneNumber = phoneNumber;
                PhoneNumberConfirmed = false;
            }
        }

        [PropertyAlterer(nameof(NormalizedUsername))]
        [PropertyAlterer(nameof(Username))]
        public virtual void SetUsername(string username)
        {
            if (!Regex.IsMatch(username, UsernameRegex))
                throw new ArgumentOutOfRangeException(nameof(username));

            if (Username != username)
            {
                NormalizedUsername = NormalizeUsername(username);
                Username = username;
            }
        }

        // Helpers.
        private string NormalizeEmail(string email)
        {
            if (email is null)
                throw new ArgumentNullException(nameof(email));

            email = email.ToUpper(CultureInfo.InvariantCulture); //to upper case

            var components = email.Split('@');
            var username = components[0];
            var domain = components[1];

            var cleanedUsername = username.Split('+')[0]; //remove chars after '+' symbol, if present

            return $"{cleanedUsername}@{domain}";
        }

        private string NormalizeUsername(string username)
        {
            if (username is null)
                throw new ArgumentNullException(nameof(username));

            username = username.ToUpper(CultureInfo.InvariantCulture); //to upper case

            return username;
        }
    }
}
