﻿//   Copyright 2021-present Etherna Sagl
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
using System.Linq;

namespace Etherna.SSOServer.Domain.Models
{
    public class UserWeb2 : UserBase
    {
        // Fields.
        private Account? _etherManagedAccount;
        private List<UserAgg.UserLoginInfo> _logins = new();
        private List<string> _twoFactorRecoveryCode = new();

        // Constructors.
        public UserWeb2(
            string username,
            UserBase? invitedBy,
            bool invitedByAdmin,
            string etherManagedPrivateKey,
            UserSharedInfo sharedInfo,
            UserAgg.UserLoginInfo? loginInfo = default)
            : base(username, invitedBy, invitedByAdmin, sharedInfo)
        {
            // Verify that private key generates correct ether address.
            var account = new Account(etherManagedPrivateKey);
            if (account.Address != sharedInfo.EtherAddress)
                throw new ArgumentException("Ethereum managed private key doesn't generate same address of shared info");

            // Initialize.
            if (loginInfo is not null)
                AddLogin(loginInfo);

            EtherManagedPrivateKey = etherManagedPrivateKey;
        }
        protected UserWeb2() { }

        // Properties.
        public virtual int AccessFailedCount { get; protected set; }
        public virtual string? AuthenticatorKey { get; set; }
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
        public override string EtherAddress
        {
            get => EtherManagedAccount.Address;
            protected set { } //disable set for managed account
        }
        public virtual Account EtherManagedAccount
        {
            get
            {
                if (_etherManagedAccount is null)
                    _etherManagedAccount = new Account(EtherManagedPrivateKey);

                return _etherManagedAccount;
            }
        }
        public virtual string EtherManagedPrivateKey { get; protected set; } = default!;
        [PersonalData]
        public virtual string? EtherLoginAddress { get; protected set; }
        public virtual bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
        [PersonalData]
        public virtual IEnumerable<UserAgg.UserLoginInfo> Logins
        {
            get => _logins;
            protected set => _logins = new List<UserAgg.UserLoginInfo>(value ?? Array.Empty<UserAgg.UserLoginInfo>());
        }
        public virtual string? PasswordHash { get; set; }
        public virtual bool TwoFactorEnabled { get; set; }
        public virtual IEnumerable<string> TwoFactorRecoveryCodes
        {
            get => _twoFactorRecoveryCode;
            set => _twoFactorRecoveryCode = new List<string>(value ?? Array.Empty<string>());
        }

        // Methods.
        [PropertyAlterer(nameof(Logins))]
        public virtual bool AddLogin(UserAgg.UserLoginInfo userLoginInfo)
        {
            //avoid multiaccounting with same provider
            if (Logins.Any(login => login.LoginProvider == userLoginInfo.ProviderKey))
                return false;

            _logins.Add(userLoginInfo);
            return true;
        }

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

        [PropertyAlterer(nameof(Logins))]
        public virtual bool RemoveExternalLogin(string loginProvider, string providerKey)
        {
            if (!CanRemoveExternalLogin)
                throw new InvalidOperationException();

            return _logins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey) > 0;
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
