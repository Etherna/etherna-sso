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

using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Etherna.SSOServer.Domain.Models
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class RoleTest
    {
        // Tests.
        [Fact]
        public void SetName_WithNewName_UpdatesNameAndNormalizedName()
        {
            // Arrange.
            var role = new Role("User");

            // Action.
            role.SetName("Administrator");

            // Assert.
            Assert.Equal("Administrator", role.Name);
            Assert.Equal("ADMINISTRATOR", role.NormalizedName);
        }

        [Fact]
        public void SetName_WithSameName_DoesNotAlterValues()
        {
            // Arrange.
            var role = new Role("User");

            // Action.
            role.SetName("User");

            // Assert.
            Assert.Equal("User", role.Name);
            Assert.Equal("USER", role.NormalizedName);
        }

        [Theory]
        [InlineData("User", "USER")]
        [InlineData("Administrator", "ADMINISTRATOR")]
        [InlineData("moderator", "MODERATOR")]
        [InlineData("ALREADY_UPPER", "ALREADY_UPPER")]
        public void NormalizeName_WithAnyCasing_ReturnsUpperCase(string input, string expected)
        {
            // Action.
            var result = Role.NormalizeName(input);

            // Assert.
            Assert.Equal(expected, result);
        }
    }
}
