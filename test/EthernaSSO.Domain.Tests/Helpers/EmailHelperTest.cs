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

namespace Etherna.SSOServer.Domain.Helpers
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class EmailHelperTest
    {
        // Tests.
        [Theory]
        [InlineData("notanemail")]
        [InlineData("user@example.com@evil.org")]
        [InlineData("@example.com")]
        [InlineData("user@")]
        public void NormalizeEmail_WithMalformedEmail_ThrowsArgumentException(string email)
        {
            // Action & Assert.
            Assert.Throws<ArgumentException>(() => EmailHelper.NormalizeEmail(email));
        }
    }
}
