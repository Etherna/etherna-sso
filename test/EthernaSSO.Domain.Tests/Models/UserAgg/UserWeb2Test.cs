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

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class UserWeb2Test
    {
        // Helpers.
        private static UserWeb2 CreateUser() => UserWeb2Builder.Build();

        // Tests.
        [Fact]
        public void IncrementAccessFailedCount_WhenCalled_IncrementsCounter()
        {
            // Arrange.
            var user = CreateUser();
            var initialCount = user.AccessFailedCount;

            // Action.
            user.IncrementAccessFailedCount();

            // Assert.
            Assert.Equal(initialCount + 1, user.AccessFailedCount);
        }

        [Fact]
        public void IncrementAccessFailedCount_CalledMultipleTimes_AccumulatesCount()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            user.IncrementAccessFailedCount();
            user.IncrementAccessFailedCount();
            user.IncrementAccessFailedCount();

            // Assert.
            Assert.Equal(3, user.AccessFailedCount);
        }

        [Fact]
        public void ResetAccessFailedCount_WhenCalled_ResetsCounterToZero()
        {
            // Arrange.
            var user = CreateUser();
            user.IncrementAccessFailedCount();
            user.IncrementAccessFailedCount();

            // Action.
            user.ResetAccessFailedCount();

            // Assert.
            Assert.Equal(0, user.AccessFailedCount);
        }

        [Fact]
        public void RedeemTwoFactorRecoveryCode_WithExistingCode_RemovesCodeAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            user.TwoFactorRecoveryCodes = ["code1", "code2", "code3"];

            // Action.
            var result = user.RedeemTwoFactorRecoveryCode("code2");

            // Assert.
            Assert.True(result);
            Assert.DoesNotContain("code2", user.TwoFactorRecoveryCodes);
            Assert.Equal(2, System.Linq.Enumerable.Count(user.TwoFactorRecoveryCodes));
        }

        [Fact]
        public void RedeemTwoFactorRecoveryCode_WithNonExistingCode_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();
            user.TwoFactorRecoveryCodes = ["code1", "code2"];

            // Action.
            var result = user.RedeemTwoFactorRecoveryCode("nonexistent");

            // Assert.
            Assert.False(result);
            Assert.Equal(2, System.Linq.Enumerable.Count(user.TwoFactorRecoveryCodes));
        }

        [Fact]
        public void RemoveEtherLoginAddress_WithLoginAddressAndAlternativeLogin_RemovesAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            user.EtherLoginAddress = "0xfeF78523191CC15e287b3F7ABFbd0c3d621f053b";
            user.PasswordHash = "somehash"; // enables CanLoginWithUsername (NormalizedUsername is set in ctor)

            // Action.
            var result = user.RemoveEtherLoginAddress();

            // Assert.
            Assert.True(result);
            Assert.Null(user.EtherLoginAddress);
        }

        [Fact]
        public void RemoveEtherLoginAddress_WithoutLoginAddress_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            var result = user.RemoveEtherLoginAddress();

            // Assert.
            Assert.False(result);
        }

        [Fact]
        public void RemoveEtherLoginAddress_WhenOnlyLoginMethod_ThrowsInvalidOperationException()
        {
            // Arrange.
            var user = CreateUser();
            user.EtherLoginAddress = "0xfeF78523191CC15e287b3F7ABFbd0c3d621f053b";
            // No PasswordHash set, so CanLoginWithEmail/Username = false
            // => CanRemoveEtherLoginAddress = false => throws

            // Action & Assert.
            Assert.Throws<InvalidOperationException>(() => user.RemoveEtherLoginAddress());
        }

        [Fact]
        public void TwoFactorEnabled_WithNoFactors_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();

            // Assert.
            Assert.False(user.TwoFactorEnabled);
            Assert.False(user.IsAuthenticatorAppEnabled);
            Assert.False(user.HasFido2Credentials);
        }

        [Fact]
        public void TwoFactorEnabled_WithUnconfirmedAuthenticatorKey_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();
            user.AuthenticatorKey = "some-key";

            // Assert. A stored key alone (setup started, not verified) must not enable 2FA.
            Assert.False(user.TwoFactorEnabled);
            Assert.False(user.IsAuthenticatorAppEnabled);
        }

        [Fact]
        public void EnableAuthenticatorApp_WithAuthenticatorKey_EnablesTwoFactor()
        {
            // Arrange.
            var user = CreateUser();
            user.AuthenticatorKey = "some-key";

            // Act.
            user.EnableAuthenticatorApp();

            // Assert.
            Assert.True(user.IsAuthenticatorAppEnabled);
            Assert.True(user.TwoFactorEnabled);
        }

        [Fact]
        public void EnableAuthenticatorApp_WithoutAuthenticatorKey_Throws()
        {
            // Arrange.
            var user = CreateUser();

            // Assert.
            Assert.Throws<InvalidOperationException>(user.EnableAuthenticatorApp);
        }

        [Fact]
        public void DisableAuthenticatorApp_WithEnabledAuthenticatorApp_ClearsKeyAndDisables()
        {
            // Arrange.
            var user = CreateUser();
            user.AuthenticatorKey = "some-key";
            user.EnableAuthenticatorApp();

            // Act.
            user.DisableAuthenticatorApp();

            // Assert.
            Assert.Null(user.AuthenticatorKey);
            Assert.False(user.IsAuthenticatorAppEnabled);
            Assert.False(user.TwoFactorEnabled);
        }

        [Fact]
        public void TwoFactorEnabled_WithFido2Credential_ReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            user.AddFido2Credential(UserWeb2Builder.BuildFido2Credential());

            // Assert.
            Assert.True(user.TwoFactorEnabled);
            Assert.True(user.HasFido2Credentials);
        }

        [Fact]
        public void AddFido2Credential_WithNewCredential_AddsAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            var credential = UserWeb2Builder.BuildFido2Credential();

            // Action.
            var result = user.AddFido2Credential(credential);

            // Assert.
            Assert.True(result);
            Assert.Single(user.Fido2Credentials);
        }

        [Fact]
        public void AddFido2Credential_WithDuplicateCredentialId_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();
            user.AddFido2Credential(UserWeb2Builder.BuildFido2Credential(credentialId: [9, 9, 9]));

            // Action.
            var result = user.AddFido2Credential(UserWeb2Builder.BuildFido2Credential(credentialId: [9, 9, 9], nickname: "other"));

            // Assert.
            Assert.False(result);
            Assert.Single(user.Fido2Credentials);
        }

        [Fact]
        public void RemoveFido2Credential_WithExistingId_RemovesAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            user.AddFido2Credential(UserWeb2Builder.BuildFido2Credential(credentialId: [1, 1]));
            user.AddFido2Credential(UserWeb2Builder.BuildFido2Credential(credentialId: [2, 2], nickname: "k2"));

            // Action.
            var result = user.RemoveFido2Credential([1, 1]);

            // Assert.
            Assert.True(result);
            Assert.Single(user.Fido2Credentials);
        }

        [Fact]
        public void RemoveFido2Credential_WithUnknownId_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();
            user.AddFido2Credential(UserWeb2Builder.BuildFido2Credential(credentialId: [1]));

            // Action.
            var result = user.RemoveFido2Credential([2]);

            // Assert.
            Assert.False(result);
            Assert.Single(user.Fido2Credentials);
        }

        [Fact]
        public void RenameFido2Credential_WithExistingId_UpdatesNicknameAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            user.AddFido2Credential(UserWeb2Builder.BuildFido2Credential(credentialId: [1], nickname: "old"));

            // Action.
            var result = user.RenameFido2Credential([1], "new");

            // Assert.
            Assert.True(result);
            Assert.Equal("new", System.Linq.Enumerable.First(user.Fido2Credentials).Nickname);
        }

        [Fact]
        public void RenameFido2Credential_WithUnknownId_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            var result = user.RenameFido2Credential([9], "new");

            // Assert.
            Assert.False(result);
        }

        [Fact]
        public void RecordFido2CredentialUsage_WithExistingId_UpdatesCounter()
        {
            // Arrange.
            var user = CreateUser();
            user.AddFido2Credential(UserWeb2Builder.BuildFido2Credential(credentialId: [1], signatureCounter: 0));

            // Action.
            user.RecordFido2CredentialUsage([1], 5);

            // Assert.
            var credential = user.FindFido2Credential([1]);
            Assert.NotNull(credential);
            Assert.Equal(5u, credential.SignatureCounter);
            Assert.NotNull(credential.LastUsedAt);
        }

        [Fact]
        public void RecordFido2CredentialUsage_WithUnknownId_ThrowsInvalidOperationException()
        {
            // Arrange.
            var user = CreateUser();

            // Action & Assert.
            Assert.Throws<InvalidOperationException>(() => user.RecordFido2CredentialUsage([9], 1));
        }
    }
}
