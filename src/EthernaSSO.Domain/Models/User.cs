using Digicando.DomainHelper.Attributes;
using Etherna.SSOServer.Domain.Models.UserAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etherna.SSOServer.Domain.Models
{
    public class User : EntityModelBase<string>
    {
        // Fields.
        private List<UserLoginInfo> _logins = new List<UserLoginInfo>();

        // Constructors and dispose.
        protected User() { }

        // Builders.
        public static User CreateManagedUser(string username, string password)
        {
            //build new address
        }

        public static User CreateUnmanagedUser(string address, string signature)
        {
            //verify signature


            return new User() { Id = address };
        }

        // Properties.
        public virtual int AccessFailedCount { get; internal protected set; }
        public virtual string Address { get; protected set; }
        public virtual string Email { get; protected set; }
        public virtual bool EmailConfirmed { get; protected set; }
        public virtual bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
        public override string Id { get => Address; protected set => Address = value; } //Id == Address
        public virtual bool LockoutEnabled { get; internal protected set; }
        public virtual DateTimeOffset? LockoutEnd { get; internal protected set; }
        public virtual IEnumerable<UserLoginInfo> Logins
        {
            get => _logins;
            protected set => _logins = new List<UserLoginInfo>(value ?? new UserLoginInfo[0]);
        }
        public virtual string NormalizedEmail { get; protected set; }
        public virtual string NormalizedUserName { get; protected set; }
        public virtual string PasswordHash { get; internal protected set; }
        public virtual string SecurityStamp { get; internal protected set; }
        public virtual string UserName { get; protected set; }

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

        [PropertyAlterer(nameof(Logins))]
        public virtual void RemoveLogin(string loginProvider, string providerKey) =>
            _logins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);

        [PropertyAlterer(nameof(Email))]
        [PropertyAlterer(nameof(EmailConfirmed))]
        [PropertyAlterer(nameof(NormalizedEmail))]
        public virtual void SetEmail(string email)
        {
            if (!Regex.IsMatch(email,
                @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
                RegexOptions.IgnoreCase))
                throw new ArgumentOutOfRangeException(nameof(Email));

            if (Email != email)
            {
                Email = email;
                EmailConfirmed = false;
                NormalizedEmail = NormalizeEmail(email);
            }
        }

        [PropertyAlterer(nameof(NormalizedUserName))]
        [PropertyAlterer(nameof(UserName))]
        public virtual void SetUserName(string userName)
        {
            if (userName is null)
                throw new ArgumentNullException(nameof(userName));

            if (UserName != userName)
            {
                NormalizedUserName = NormalizeUserName(userName);
                UserName = userName;
            }
        }

        // Helpers.
        private string NormalizeEmail(string email)
        {
            if (email is null)
                throw new ArgumentNullException(nameof(email));

            email = email.ToUpper(); //to upper case

            var components = email.Split('@');
            var username = components[0];
            var domain = components[1];

            var cleanedUsername = username.Split('+')[0]; //remove chars after '+' symbol, if present

            return $"{cleanedUsername}@{domain}";
        }

        private string NormalizeUserName(string userName)
        {
            if (userName is null)
                throw new ArgumentNullException(nameof(userName));

            userName = userName.ToUpper(); //to upper case

            return userName;
        }
    }
}
