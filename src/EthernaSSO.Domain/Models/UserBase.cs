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

using Etherna.ACR.Helpers;
using Etherna.Authentication;
using Etherna.MongODM.Core.Attributes;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Etherna.SSOServer.Domain.Models
{
    public abstract class UserBase : EntityModelBase<string>
    {
        // Consts.
        public static readonly IEnumerable<string> DomainManagedClaimNames = new[]
        {
            EthernaClaimTypes.EtherAddress,
            EthernaClaimTypes.EtherPreviousAddresses,
            EthernaClaimTypes.IsWeb3Account,
            EthernaClaimTypes.Role,
            EthernaClaimTypes.Username
        };

        // Fields.
        private readonly List<UserClaim> _customClaims = new();
        private List<string> _etherPreviousAddresses = new();
        private List<Role> _roles = new();

        // Constructors.
        protected UserBase(
            string username,
            UserBase? invitedBy,
            bool invitedByAdmin,
            UserSharedInfo sharedInfo)
        {
            if (sharedInfo is null)
                throw new ArgumentNullException(nameof(sharedInfo));

            SetUsername(username);
            InvitedBy = invitedBy;
            InvitedByAdmin = invitedByAdmin;
            SharedInfoId = sharedInfo.Id;
        }
        protected UserBase() { }

        // Properties.
        public virtual IEnumerable<UserClaim> Claims
        {
            get => DefaultClaims.Union(_customClaims);
            protected set
            {
                _customClaims.Clear();
                foreach (var claim in value ?? Array.Empty<UserClaim>())
                    AddClaim(claim);
            }
        }
        public virtual IEnumerable<UserClaim> DefaultClaims
        {
            get
            {
                var claims = new List<UserClaim>
                {
                    new UserClaim(EthernaClaimTypes.EtherAddress, EtherAddress),
                    new UserClaim(EthernaClaimTypes.EtherPreviousAddresses, JsonSerializer.Serialize(_etherPreviousAddresses)),
                    new UserClaim(EthernaClaimTypes.IsWeb3Account, (this is UserWeb3).ToString()),
                    new UserClaim(EthernaClaimTypes.Username, Username)
                };

                foreach (var role in _roles)
                    claims.Add(new UserClaim(EthernaClaimTypes.Role, role.NormalizedName));

                return claims;
            }
        }

        [PersonalData]
        public virtual string? Email { get; protected set; }

        [PersonalData]
        /* Keep until SharedInfo can't be encapsulated. */
        public virtual string EtherAddress { get; protected set; } = default!;
        //[PersonalData]
        //public virtual string EtherAddress => SharedInfo.EtherAddress;

        [PersonalData]
        /* Keep until SharedInfo can't be encapsulated. */
        public virtual IEnumerable<string> EtherPreviousAddresses
        {
            get => _etherPreviousAddresses;
            protected set => _etherPreviousAddresses = new List<string>(value ?? Array.Empty<string>());
        }
        //[PersonalData]
        //public virtual IEnumerable<string> EtherPreviousAddresses => SharedInfo.EtherPreviousAddresses;

        public virtual UserBase? InvitedBy { get; protected set; }
        public virtual bool InvitedByAdmin { get; protected set; }
        [PersonalData]
        public virtual DateTime LastLoginDateTime { get; protected set; }
        //public virtual bool LockoutEnabled => SharedInfo.LockoutEnabled;
        //public virtual DateTimeOffset? LockoutEnd => SharedInfo.LockoutEnd;
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

        /* SharedInfo is encapsulable with resolution of https://etherna.atlassian.net/browse/MODM-101.
         * With encapsulation we can expose also EtherAddress and EtherPreviousAddresses properties
         * pointing to SharedInfo internal property, and avoid data duplication.
         */
        //protected abstract SharedUserInfo SharedInfo { get; set; }
        public virtual string SharedInfoId { get; protected set; } = default!;

        [PersonalData]
        public virtual string Username { get; protected set; } = default!;

        // Methods.
        [PropertyAlterer(nameof(Claims))]
        public virtual bool AddClaim(UserClaim claim)
        {
            if (claim is null)
                throw new ArgumentNullException(nameof(claim));

            //keep default claims managed by model
            if (DomainManagedClaimNames.Contains(claim.Type))
                return false;

            //don't add duplicate claims
            if (_customClaims.Any(c => c.Type == claim.Type &&
                                       c.Value == claim.Value))
                return false;

            _customClaims.Add(claim);
            return true;
        }

        [PropertyAlterer(nameof(Roles))]
        public virtual bool AddRole(Role role)
        {
            if (!_roles.Contains(role))
            {
                _roles.Add(role);
                return true;
            }
            return false;
        }

        [PropertyAlterer(nameof(PhoneNumberConfirmed))]
        public virtual void ConfirmPhoneNumber()
        {
            if (PhoneNumber is null)
                throw new InvalidOperationException();

            PhoneNumberConfirmed = true;
        }

        [PropertyAlterer(nameof(Claims))]
        public virtual bool RemoveClaim(UserClaim claim)
        {
            if (claim is null)
                throw new ArgumentNullException(nameof(claim));

            return RemoveClaim(claim.Type, claim.Value);
        }

        [PropertyAlterer(nameof(Claims))]
        public virtual bool RemoveClaim(string type, string value)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var removed = _customClaims.RemoveAll(c => c.Type == type &&
                                                       c.Value == value);
            return removed > 0;
        }

        [PropertyAlterer(nameof(Email))]
        [PropertyAlterer(nameof(NormalizedEmail))]
        public virtual bool RemoveEmail()
        {
            if (Email is null)
                return false;

            Email = null;
            NormalizedEmail = null;

            return true;
        }

        [PropertyAlterer(nameof(Roles))]
        public virtual bool RemoveRole(string roleName) =>
            _roles.RemoveAll(r => r.NormalizedName == roleName || r.Name == roleName) > 0;

        [PropertyAlterer(nameof(Email))]
        [PropertyAlterer(nameof(NormalizedEmail))]
        public virtual void SetEmail(string email)
        {
            if (!EmailHelper.IsValidEmail(email))
                throw new ArgumentException("Email is not valid", nameof(email));

            if (Email != email)
            {
                Email = email;
                NormalizedEmail = EmailHelper.NormalizeEmail(email);
            }
        }

        [PropertyAlterer(nameof(PhoneNumber))]
        [PropertyAlterer(nameof(PhoneNumberConfirmed))]
        public virtual void SetPhoneNumber(string? phoneNumber)
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
            if (!UsernameHelper.IsValidUsername(username))
                throw new ArgumentException("Username is not valid", nameof(username));

            if (Username != username)
            {
                NormalizedUsername = UsernameHelper.NormalizeUsername(username);
                Username = username;
            }
        }

        [PropertyAlterer(nameof(LastLoginDateTime))]
        public virtual void UpdateLastLoginDateTime()
        {
            LastLoginDateTime = DateTime.UtcNow;
        }
    }
}
