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

using Etherna.SSOServer.Domain.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Etherna.SSOServer.Domain.Models.Fido2CredentialAgg
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class Fido2CredentialTest
    {
        [Fact]
        public void Constructor_WithEmptyCredentialId_ThrowsArgumentException()
        {
            // Action & Assert.
            Assert.Throws<ArgumentException>(() =>
                new Fido2Credential([], [1, 2, 3], 0, "nick", ["usb"]));
        }

        [Fact]
        public void Constructor_WithEmptyPublicKey_ThrowsArgumentException()
        {
            // Action & Assert.
            Assert.Throws<ArgumentException>(() =>
                new Fido2Credential([1, 2, 3], [], 0, "nick", ["usb"]));
        }

        [Fact]
        public void Constructor_WithEmptyNickname_ThrowsArgumentException()
        {
            // Action & Assert.
            Assert.Throws<ArgumentException>(() =>
                new Fido2Credential([1], [2], 0, "", null));
        }

        [Fact]
        public void Constructor_WithTooLongNickname_ThrowsArgumentException()
        {
            // Arrange.
            var nickname = new string('a', Fido2Credential.MaxNicknameLength + 1);

            // Action & Assert.
            Assert.Throws<ArgumentException>(() =>
                new Fido2Credential([1], [2], 0, nickname, null));
        }

        [Fact]
        public void Constructor_WithNullTransports_DefaultsToEmpty()
        {
            // Action.
            var credential = new Fido2Credential([1], [2], 0, "nick", null);

            // Assert.
            Assert.Empty(credential.Transports);
        }

        [Fact]
        public void RecordUsage_WithGreaterCounter_UpdatesCounterAndTimestamp()
        {
            // Arrange.
            var credential = UserWeb2Builder.BuildFido2Credential(signatureCounter: 5);

            // Action.
            credential.RecordUsage(10);

            // Assert.
            Assert.Equal(10u, credential.SignatureCounter);
            Assert.NotNull(credential.LastUsedAt);
        }

        [Fact]
        public void RecordUsage_WithZeroCounterWhenStoredIsZero_Allowed()
        {
            // Arrange.
            var credential = UserWeb2Builder.BuildFido2Credential(signatureCounter: 0);

            // Action.
            credential.RecordUsage(0);

            // Assert.
            Assert.Equal(0u, credential.SignatureCounter);
            Assert.NotNull(credential.LastUsedAt);
        }

        [Fact]
        public void RecordUsage_WithRegressedCounter_ThrowsInvalidOperationException()
        {
            // Arrange.
            var credential = UserWeb2Builder.BuildFido2Credential(signatureCounter: 10);

            // Action & Assert.
            Assert.Throws<InvalidOperationException>(() => credential.RecordUsage(5));
        }

        [Fact]
        public void RecordUsage_WithZeroCounterWhenStoredIsNonzero_ThrowsInvalidOperationException()
        {
            // Arrange. The authenticator already proved a working counter; a later 0 is a clone signal.
            var credential = UserWeb2Builder.BuildFido2Credential(signatureCounter: 10);

            // Action & Assert.
            Assert.Throws<InvalidOperationException>(() => credential.RecordUsage(0));
            Assert.Equal(10u, credential.SignatureCounter); //baseline must be preserved
        }

        [Fact]
        public void SetNickname_WithValidValue_UpdatesNickname()
        {
            // Arrange.
            var credential = UserWeb2Builder.BuildFido2Credential();

            // Action.
            credential.SetNickname("renamed");

            // Assert.
            Assert.Equal("renamed", credential.Nickname);
        }

        [Fact]
        public void SetNickname_WithEmptyValue_ThrowsArgumentException()
        {
            // Arrange.
            var credential = UserWeb2Builder.BuildFido2Credential();

            // Action & Assert.
            Assert.Throws<ArgumentException>(() => credential.SetNickname(""));
        }
    }
}
