using Etherna.MongODM.Core.Attributes;
using Microsoft.AspNetCore.Identity;
using Nethereum.Hex.HexConvertors.Extensions;
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
        public UserWeb2(string username, UserBase? invitedBy, UserAgg.UserLoginInfo? loginInfo = default)
            : base(username, invitedBy)
        {
            if (loginInfo is not null)
                AddLogin(loginInfo);

            EtherManagedPrivateKey = GenerateEtherPrivateKey();
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
        public virtual string? EtherManagedPrivateKey { get; protected set; }
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

        // Private helpers.
        private static string GenerateEtherPrivateKey()
        {
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            return privateKey;
        }
    }
}
