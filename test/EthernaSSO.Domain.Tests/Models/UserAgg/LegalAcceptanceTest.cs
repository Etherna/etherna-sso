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

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class LegalAcceptanceTest
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsProperties()
        {
            // Arrange.
            var acceptanceDateTime = new DateTime(2026, 06, 25, 10, 30, 00, DateTimeKind.Utc);

            // Action.
            var acceptance = new LegalAcceptance(LegalDocumentType.TermsOfService, "2026-06-25", acceptanceDateTime);

            // Assert.
            Assert.Equal(LegalDocumentType.TermsOfService, acceptance.DocumentType);
            Assert.Equal("2026-06-25", acceptance.Version);
            Assert.Equal(acceptanceDateTime, acceptance.AcceptanceDateTime);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithEmptyVersion_ThrowsArgumentException(string? version)
        {
            // Assert.
            Assert.Throws<ArgumentException>(() =>
                new LegalAcceptance(LegalDocumentType.PrivacyPolicy, version!, DateTime.UtcNow));
        }

        [Fact]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange.
            var acceptanceDateTime = new DateTime(2026, 06, 25, 10, 30, 00, DateTimeKind.Utc);
            var a = new LegalAcceptance(LegalDocumentType.TermsOfService, "2026-06-25", acceptanceDateTime);
            var b = new LegalAcceptance(LegalDocumentType.TermsOfService, "2026-06-25", acceptanceDateTime);

            // Assert.
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equals_WithDifferentVersion_ReturnsFalse()
        {
            // Arrange.
            var acceptanceDateTime = new DateTime(2026, 06, 25, 10, 30, 00, DateTimeKind.Utc);
            var a = new LegalAcceptance(LegalDocumentType.TermsOfService, "2026-06-25", acceptanceDateTime);
            var b = new LegalAcceptance(LegalDocumentType.TermsOfService, "2026-01-01", acceptanceDateTime);

            // Assert.
            Assert.NotEqual(a, b);
        }
    }
}
