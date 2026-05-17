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
    }
}
