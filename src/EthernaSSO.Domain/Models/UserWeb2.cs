// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.MongODM.Core.Attributes;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Microsoft.AspNetCore.Identity;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Domain.Models
{
    public class UserWeb2 : UserBase
    {
        // Fields.
        private Account? _etherManagedAccount;
        private List<string> _twoFactorRecoveryCode = new();

        // Constructors.
        public UserWeb2(
            string username,
            UserBase? invitedBy,
            bool invitedByAdmin,
            string etherManagedPrivateKey,
            UserSharedInfo sharedInfo)
            : base(username, invitedBy, invitedByAdmin, sharedInfo)
        {
            // Verify that private key generates correct ether address.
            var account = new Account(etherManagedPrivateKey);
            if (account.Address != sharedInfo.EtherAddress)
                throw new ArgumentException("Ethereum managed private key doesn't generate same address of shared info");

            EtherManagedPrivateKey = etherManagedPrivateKey;
        }
        protected UserWeb2() { }

        // Properties.
        public virtual int AccessFailedCount { get; protected set; }
        public virtual string? AuthenticatorKey { get; set; }
        public virtual bool CanLoginWithEmail => NormalizedEmail != null && PasswordHash != null;
        public virtual bool CanLoginWithEtherAddress => EtherLoginAddress != null;
        public virtual bool CanLoginWithUsername => NormalizedUsername != null && PasswordHash != null;
        public virtual bool CanRemoveEtherLoginAddress =>
            CanLoginWithEtherAddress &&
                (CanLoginWithEmail ||
                 CanLoginWithUsername);
        public override string EtherAddress
        {
            get => EtherManagedAccount.Address;
            protected set { } //disable set for managed account
        }
        public virtual Account EtherManagedAccount => _etherManagedAccount ??= new Account(EtherManagedPrivateKey);
        public virtual string EtherManagedPrivateKey { get; protected set; } = default!;
        [PersonalData]
        public virtual string? EtherLoginAddress { get; protected set; }
        public virtual bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
        public virtual string? PasswordHash { get; set; }
        public virtual bool TwoFactorEnabled { get; set; }
        public virtual IEnumerable<string> TwoFactorRecoveryCodes
        {
            get => _twoFactorRecoveryCode;
            set => _twoFactorRecoveryCode = new List<string>(value ?? Array.Empty<string>());
        }

        // Methods.
        [PropertyAlterer(nameof(AccessFailedCount))]
        public virtual void IncrementAccessFailedCount() => AccessFailedCount++;

        [PropertyAlterer(nameof(TwoFactorRecoveryCodes))]
        public virtual bool RedeemTwoFactorRecoveryCode(string code) =>
            _twoFactorRecoveryCode.Remove(code);

        [PropertyAlterer(nameof(EtherLoginAddress))]
        public virtual bool RemoveEtherLoginAddress()
        {
            if (EtherLoginAddress is null)
                return false;

            if (!CanRemoveEtherLoginAddress)
                throw new InvalidOperationException();

            EtherLoginAddress = null;

            return true;
        }

        [PropertyAlterer(nameof(AccessFailedCount))]
        public virtual void ResetAccessFailedCount() => AccessFailedCount = 0;

        [PropertyAlterer(nameof(EtherLoginAddress))]
        public virtual void SetEtherLoginAddress(string address)
        {
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            EtherLoginAddress = address.ConvertToEthereumChecksumAddress();
        }
    }
}
