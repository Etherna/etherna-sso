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

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Etherna.SSOServer.Domain.Models
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class AlphaPassRequestTest
    {
        // Tests.
        [Fact]
        public void ConfirmEmail_WithCorrectSecret_SetsEmailConfirmedAndReturnsTrue()
        {
            // Arrange.
            var request = new AlphaPassRequest("test@example.com");

            // Action.
            var result = request.ConfirmEmail(request.Secret);

            // Assert.
            Assert.True(result);
            Assert.True(request.IsEmailConfirmed);
        }

        [Fact]
        public void ConfirmEmail_WithWrongSecret_ReturnsFalseAndLeavesUnconfirmed()
        {
            // Arrange.
            var request = new AlphaPassRequest("test@example.com");

            // Action.
            var result = request.ConfirmEmail("wrongsecret");

            // Assert.
            Assert.False(result);
            Assert.False(request.IsEmailConfirmed);
        }

        [Fact]
        public void Constructor_WithInvalidEmail_ThrowsArgumentException()
        {
            // Action & Assert.
            Assert.Throws<ArgumentException>(() => new AlphaPassRequest("not-an-email"));
        }

        [Fact]
        public void Constructor_WithValidEmail_NormalizesEmail()
        {
            // Arrange & Action.
            var request = new AlphaPassRequest("Test+alias@Example.COM");

            // Assert.
            Assert.Equal("TEST@EXAMPLE.COM", request.NormalizedEmail);
        }

        [Fact]
        public void Constructor_WithValidEmail_GeneratesNonEmptySecret()
        {
            // Arrange & Action.
            var request = new AlphaPassRequest("test@example.com");

            // Assert.
            Assert.NotNull(request.Secret);
            Assert.NotEmpty(request.Secret);
            Assert.Equal(AlphaPassRequest.CodeLength, request.Secret.Length);
        }
    }
}
