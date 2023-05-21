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
        public virtual bool RedeemTwoFactorRecoveryCode(string code)
        {
            if (_twoFactorRecoveryCode.Contains(code))
            {
                _twoFactorRecoveryCode.Remove(code);
                return true;
            }
            return false;
        }

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
