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
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Domain.Models.Fido2CredentialAgg
{
    public class Fido2Credential : ModelBase
    {
        // Consts.
        public const int MaxNicknameLength = 64;

        // Fields.
        private List<string> _transports = [];

        // Constructors.
        public Fido2Credential(
            byte[] credentialId,
            byte[] publicKey,
            uint signatureCounter,
            string nickname,
            IEnumerable<string>? transports)
        {
            ArgumentNullException.ThrowIfNull(credentialId);
            ArgumentNullException.ThrowIfNull(publicKey);
            if (credentialId.Length == 0)
                throw new ArgumentException("CredentialId can't be empty", nameof(credentialId));
            if (publicKey.Length == 0)
                throw new ArgumentException("PublicKey can't be empty", nameof(publicKey));

            CreatedAt = DateTime.UtcNow;
            CredentialId = credentialId;
            PublicKey = publicKey;
            SignatureCounter = signatureCounter;
            Transports = transports ?? [];
            SetNickname(nickname);
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected Fido2Credential() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        // Properties.
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual byte[] CredentialId { get; protected set; }
        public virtual DateTime? LastUsedAt { get; protected set; }
        public virtual string Nickname { get; protected set; } = null!;
        public virtual byte[] PublicKey { get; protected set; }
        public virtual uint SignatureCounter { get; protected set; }
        public virtual IEnumerable<string> Transports
        {
            get => _transports;
            protected set => _transports = [..value ?? []];
        }

        // Methods.
        [PropertyAlterer(nameof(LastUsedAt))]
        [PropertyAlterer(nameof(SignatureCounter))]
        public virtual void RecordUsage(uint newCounter)
        {
            //Anti-clone check (WebAuthn §6.1.1 / §7.2 step 21): the counter is only meaningless when BOTH the
            //stored and the new value are 0 (authenticator that never implements it). As soon as either is
            //nonzero the value must strictly increase — a non-increasing one, including a drop back to 0 after
            //a real count, signals a possible cloned authenticator and must not overwrite the stored baseline.
            if ((newCounter != 0 || SignatureCounter != 0) && newCounter <= SignatureCounter)
                throw new InvalidOperationException("Signature counter regression detected; possible cloned authenticator.");

            SignatureCounter = newCounter;
            LastUsedAt = DateTime.UtcNow;
        }

        [PropertyAlterer(nameof(Nickname))]
        public virtual void SetNickname(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                throw new ArgumentException("Nickname can't be empty", nameof(nickname));
            if (nickname.Length > MaxNicknameLength)
                throw new ArgumentException($"Nickname can't exceed {MaxNicknameLength} characters", nameof(nickname));

            Nickname = nickname;
        }
    }
}
