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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Etherna.SSOServer.Domain.Models
{
    public abstract class UserBase : EntityModelBase<string>
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
        private List<string> _etherPreviousAddresses = new();
        private List<Role> _roles = new();

        // Constructors.
        protected UserBase(string username, string? email = default)
        {
            SetUsername(username);
            if (email != null)
                SetEmail(email);
        }
        protected UserBase() { }

        // Properties.
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
        public virtual string EtherAddress { get; protected set; } = default!;
        [PersonalData]
        public virtual IEnumerable<string> EtherPreviousAddresses
        {
            get => _etherPreviousAddresses;
            protected set => _etherPreviousAddresses = new List<string>(value ?? Array.Empty<string>());
        }
        [PersonalData]
        public virtual DateTime LastLoginDateTime { get; protected set; }
        public virtual bool LockoutEnabled { get; set; }
        public virtual DateTimeOffset? LockoutEnd { get; set; }
        public virtual string? NormalizedEmail { get; protected set; }
        public virtual string NormalizedUsername { get; protected set; } = default!;
        [PersonalData]
        public virtual string? PhoneNumber { get; protected set; }
        public virtual bool PhoneNumberConfirmed { get; protected set; }
        public virtual IEnumerable<Role> Roles
        {
            get => _roles;
            protected set => _roles = new List<Role>(value ?? Array.Empty<Role>());
        }
        public virtual string SecurityStamp { get; set; } = default!;
        [PersonalData]
        public virtual string Username { get; protected set; } = default!;

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

        [PropertyAlterer(nameof(Roles))]
        public virtual void AddRole(Role role)
        {
            if (!_roles.Contains(role))
                _roles.Add(role);
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

        [PropertyAlterer(nameof(Claims))]
        public virtual void RemoveClaims(IEnumerable<UserClaim> claims)
        {
            if (claims is null)
                throw new ArgumentNullException(nameof(claims));

            foreach (var claim in claims)
                _customClaims.RemoveAll(c => c.Type == claim.Type &&
                                             c.Value == claim.Value);
        }

        [PropertyAlterer(nameof(Email))]
        [PropertyAlterer(nameof(EmailConfirmed))]
        [PropertyAlterer(nameof(NormalizedEmail))]
        public virtual bool RemoveEmail()
        {
            if (Email is null)
                return false;

            Email = null;
            NormalizedEmail = null;
            EmailConfirmed = false;

            return true;
        }

        [PropertyAlterer(nameof(Roles))]
        public virtual bool RemoveRole(string roleName) =>
            _roles.RemoveAll(r => r.Name == roleName) > 0;

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

        [PropertyAlterer(nameof(LastLoginDateTime))]
        public virtual void UpdateLastLoginDateTime()
        {
            LastLoginDateTime = DateTime.Now;
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
    }
}
