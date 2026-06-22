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

using Etherna.BeeNet.Models;
using Etherna.MongODM.Core.Attributes;
using Etherna.SSOServer.Domain.Models.Fido2CredentialAgg;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using Account = Nethereum.Web3.Accounts.Account;

namespace Etherna.SSOServer.Domain.Models
{
    public class UserWeb2 : UserBase
    {
        // Fields.
        private Account? _etherManagedAccount;
        private List<Fido2Credential> _fido2Credentials = [];
        private List<string> _twoFactorRecoveryCode = [];

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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected UserWeb2() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

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
        public override EthAddress EtherAddress
        {
            get => EtherManagedAccount.Address;
            protected set { } //disable set for managed account
        }
        public virtual Account EtherManagedAccount => _etherManagedAccount ??= new Account(EtherManagedPrivateKey);
        public virtual string EtherManagedPrivateKey { get; protected set; }
        [PersonalData]
        public virtual EthAddress? EtherLoginAddress { get; set; }
        public virtual IEnumerable<Fido2Credential> Fido2Credentials
        {
            get => _fido2Credentials;
            protected set => _fido2Credentials = [..value ?? []];
        }
        public virtual bool HasFido2Credentials => _fido2Credentials.Count > 0;
        public virtual bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
        public virtual bool IsAuthenticatorAppEnabled { get; protected set; }
        public virtual string? PasswordHash { get; set; }
        public virtual bool TwoFactorEnabled => IsAuthenticatorAppEnabled || HasFido2Credentials;
        public virtual IEnumerable<string> TwoFactorRecoveryCodes
        {
            get => _twoFactorRecoveryCode;
            set => _twoFactorRecoveryCode = [..value ?? []];
        }

        // Methods.
        [PropertyAlterer(nameof(Fido2Credentials))]
        public virtual bool AddFido2Credential(Fido2Credential credential)
        {
            ArgumentNullException.ThrowIfNull(credential);

            if (_fido2Credentials.Any(c => c.CredentialId.SequenceEqual(credential.CredentialId)))
                return false;

            _fido2Credentials.Add(credential);
            return true;
        }

        [PropertyAlterer(nameof(AuthenticatorKey))]
        [PropertyAlterer(nameof(IsAuthenticatorAppEnabled))]
        public virtual void DisableAuthenticatorApp()
        {
            AuthenticatorKey = null;
            IsAuthenticatorAppEnabled = false;
        }

        [PropertyAlterer(nameof(IsAuthenticatorAppEnabled))]
        public virtual void EnableAuthenticatorApp()
        {
            if (string.IsNullOrEmpty(AuthenticatorKey))
                throw new InvalidOperationException("Cannot enable the authenticator app before its key is set.");

            IsAuthenticatorAppEnabled = true;
        }

        public virtual Fido2Credential? FindFido2Credential(byte[] credentialId)
        {
            ArgumentNullException.ThrowIfNull(credentialId);
            return _fido2Credentials.FirstOrDefault(c => c.CredentialId.SequenceEqual(credentialId));
        }

        [PropertyAlterer(nameof(AccessFailedCount))]
        public virtual void IncrementAccessFailedCount() => AccessFailedCount++;

        [PropertyAlterer(nameof(Fido2Credentials))]
        public virtual void RecordFido2CredentialUsage(byte[] credentialId, uint newCounter)
        {
            var credential = FindFido2Credential(credentialId) ??
                throw new InvalidOperationException("FIDO2 credential not found for this user.");
            credential.RecordUsage(newCounter);
        }

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

        [PropertyAlterer(nameof(Fido2Credentials))]
        public virtual bool RemoveFido2Credential(byte[] credentialId)
        {
            ArgumentNullException.ThrowIfNull(credentialId);
            return _fido2Credentials.RemoveAll(c => c.CredentialId.SequenceEqual(credentialId)) > 0;
        }

        [PropertyAlterer(nameof(Fido2Credentials))]
        public virtual bool RenameFido2Credential(byte[] credentialId, string nickname)
        {
            var credential = FindFido2Credential(credentialId);
            if (credential is null)
                return false;

            credential.SetNickname(nickname);
            return true;
        }

        [PropertyAlterer(nameof(AccessFailedCount))]
        public virtual void ResetAccessFailedCount() => AccessFailedCount = 0;
    }
}
