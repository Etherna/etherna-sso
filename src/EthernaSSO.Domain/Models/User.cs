//   Copyright 2021-present Etherna Sagl
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

using Etherna.MongODM.Core.Attributes;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Microsoft.AspNetCore.Identity;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using UserLoginInfo = Etherna.SSOServer.Domain.Models.UserAgg.UserLoginInfo;

namespace Etherna.SSOServer.Domain.Models
{
    public class User : EntityModelBase<string>
    {
        // Consts.
        public const string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        public const string EmailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
        public const string UsernameRegex = "^[a-zA-Z0-9_]{5,20}$";
        public static class DefaultClaimTypes
        {
            public const string EtherAddress = "ether_address";
            public const string EtherPreviousAddress = "ether_prev_addresses";
        }

        // Fields.
        private readonly List<UserClaim> _customClaims = new();
        private Account? _etherManagedAccount;
        private List<string> _etherPreviousAddresses = new();
        private List<UserLoginInfo> _logins = new();
        private List<string> _twoFactorRecoveryCode = new();

        // Constructors and dispose.
        ///// <summary>
        ///// Web3 unmanaged user registration
        ///// </summary>
        ///// <param name="address">The user public address</param>
        //public User(string address)
        //{
        //    Address = address;
        //}
        protected User() { }

        // Static builders.
        public static User CreateManagedWithEtherLoginAddress(string loginAddress, string? email = default)
        {
            var privateKey = GenerateEtherPrivateKey();

            var user = new User { EtherManagedPrivateKey = privateKey };
            user.SetEtherLoginAddress(loginAddress);
            if (email != null) user.SetEmail(email);

            return user;
        }

        public static User CreateManagedWithExternalLogin(UserLoginInfo loginInfo, string? email = default)
        {
            var privateKey = GenerateEtherPrivateKey();

            var user = new User { EtherManagedPrivateKey = privateKey };
            user.AddLogin(loginInfo);
            if (email != null) user.SetEmail(email);

            return user;
        }

        public static User CreateManagedWithUsername(string username, string? email = default)
        {
            var privateKey = GenerateEtherPrivateKey();

            var user = new User { EtherManagedPrivateKey = privateKey };
            user.SetUsername(username);
            if (email != null) user.SetEmail(email);

            return user;
        }

        // Properties.
        public virtual int AccessFailedCount { get; internal protected set; }
        public virtual string? AuthenticatorKey { get; internal protected set; }
        public virtual bool CanLoginWithEmail => NormalizedEmail != null && PasswordHash != null;
        public virtual bool CanLoginWithEtherAddress => EtherLoginAddress != null;
        public virtual bool CanLoginWithExternalProvider => Logins.Any();
        public virtual bool CanLoginWithUsername => NormalizedUsername != null && PasswordHash != null;
        public virtual bool CanRemoveEtherLoginAddress =>
            CanLoginWithEtherAddress &&
                (CanLoginWithEmail ||
                 CanLoginWithExternalProvider ||
                 CanLoginWithUsername);
        public virtual bool CanRemoveExternalLogin =>
            CanLoginWithExternalProvider &&
                (CanLoginWithEmail ||
                 CanLoginWithEtherAddress ||
                 CanLoginWithUsername ||
                 Logins.Count() >= 2);
        public virtual IEnumerable<UserClaim> Claims
        {
            get => DefaultClaims.Union(_customClaims);
            protected set
            {
                _customClaims.Clear();
                AddClaims(value ?? Array.Empty<UserClaim>());
            }
        }
        public virtual IEnumerable<UserClaim> DefaultClaims =>
            new []
            {
                new UserClaim(DefaultClaimTypes.EtherAddress, EtherAddress),
                new UserClaim(DefaultClaimTypes.EtherPreviousAddress, JsonSerializer.Serialize(_etherPreviousAddresses))
            };
        [PersonalData]
        public virtual string? Email { get; protected set; }
        public virtual bool EmailConfirmed { get; protected set; }
        [PersonalData]
        public virtual string EtherAddress => EtherManagedAccount?.Address ??
            throw new InvalidOperationException("Can't find a valid ethereum address");
        public virtual Account? EtherManagedAccount
        {
            get
            {
                if (EtherManagedPrivateKey is null)
                    return null;

                if (_etherManagedAccount is null)
                    _etherManagedAccount = new Account(EtherManagedPrivateKey);

                return _etherManagedAccount;
            }
        }
        public virtual string? EtherManagedPrivateKey { get; protected set; } = default!;
        [PersonalData]
        public virtual IEnumerable<string> EtherPreviousAddresses
        {
            get => _etherPreviousAddresses;
            protected set => _etherPreviousAddresses = new List<string>(value ?? Array.Empty<string>());
        }
        [PersonalData]
        public virtual string? EtherLoginAddress { get; protected set; }
        public virtual bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
        public virtual bool LockoutEnabled { get; internal protected set; }
        public virtual DateTimeOffset? LockoutEnd { get; internal protected set; }
        public virtual IEnumerable<UserLoginInfo> Logins
        {
            get => _logins;
            protected set => _logins = new List<UserLoginInfo>(value ?? Array.Empty<UserLoginInfo>());
        }
        public virtual string? NormalizedEmail { get; protected set; }
        public virtual string? NormalizedUsername { get; protected set; }
        public virtual string? PasswordHash { get; internal protected set; }
        [PersonalData]
        public virtual string? PhoneNumber { get; protected set; }
        public virtual bool PhoneNumberConfirmed { get; protected set; }
        public virtual string SecurityStamp { get; internal protected set; } = default!;
        public virtual bool TwoFactorEnabled { get; internal protected set; }
        public virtual IEnumerable<string> TwoFactorRecoveryCodes
        {
            get => _twoFactorRecoveryCode;
            internal protected set => _twoFactorRecoveryCode = new List<string>(value ?? Array.Empty<string>());
        }
        [PersonalData]
        public virtual string? Username { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(Claims))]
        public virtual void AddClaims(IEnumerable<UserClaim> claims)
        {
            if (claims is null)
                throw new ArgumentNullException(nameof(claims));

            foreach (var claim in claims)
            {
                //keep default claims managed by model
                switch (claim.Type)
                {
                    case DefaultClaimTypes.EtherAddress:
                    case DefaultClaimTypes.EtherPreviousAddress:
                        continue;
                }

                //don't add duplicate claims
                if (_customClaims.Any(c => c.Type == claim.Type &&
                                           c.Value == claim.Value))
                    continue;

                _customClaims.Add(claim);
            }
        }

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
        public virtual void ConfirmEmail()
        {
            if (Email is null)
                throw new InvalidOperationException();

            EmailConfirmed = true;
        }

        [PropertyAlterer(nameof(PhoneNumberConfirmed))]
        public virtual void ConfirmPhoneNumber()
        {
            if (PhoneNumber is null)
                throw new InvalidOperationException();

            PhoneNumberConfirmed = true;
        }

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

        [PropertyAlterer(nameof(Claims))]
        public virtual void RemoveClaims(IEnumerable<UserClaim> claims)
        {
            if (claims is null)
                throw new ArgumentNullException(nameof(claims));

            foreach (var claim in claims)
                _customClaims.RemoveAll(c => c.Type == claim.Type &&
                                             c.Value == claim.Value);
        }

        [PropertyAlterer(nameof(EtherLoginAddress))]
        public virtual void RemoveEtherLoginAddress()
        {
            if (!CanRemoveEtherLoginAddress)
                throw new InvalidOperationException();

            EtherLoginAddress = null;
        }

        [PropertyAlterer(nameof(Logins))]
        public virtual void RemoveExternalLogin(string loginProvider, string providerKey)
        {
            if (!CanRemoveExternalLogin)
                throw new InvalidOperationException();

            _logins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        }

        [PropertyAlterer(nameof(EtherLoginAddress))]
        public virtual void SetEtherLoginAddress(string address)
        {
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            EtherLoginAddress = address.ConvertToEthereumChecksumAddress();
        }

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

        // Public static helpers.
        public static string NormalizeEmail(string email)
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

        public static string NormalizeUsername(string username)
        {
            if (username is null)
                throw new ArgumentNullException(nameof(username));

            username = username.ToUpper(CultureInfo.InvariantCulture); //to upper case

            return username;
        }

        // Private helpers.
        private static string GenerateEtherPrivateKey()
        {
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            return privateKey;
        }
    }
}
