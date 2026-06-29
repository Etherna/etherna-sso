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

using Etherna.Authentication;
using Etherna.SSOServer.Domain.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class UserBaseTest
    {
        // Helpers.
        private static UserWeb2 CreateUser() => UserWeb2Builder.Build();

        // Tests.
        [Fact]
        public void AddClaim_WithCustomClaim_AddsClaimAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            var claim = new UserClaim("custom_type", "custom_value");

            // Action.
            var result = user.AddClaim(claim);

            // Assert.
            Assert.True(result);
            Assert.Contains(user.Claims, c => c.Type == "custom_type" && c.Value == "custom_value");
        }

        [Fact]
        public void AddClaim_WithDomainManagedClaim_ReturnsFalseAndDoesNotAdd()
        {
            // Arrange.
            var user = CreateUser();
            var domainClaim = new UserClaim(EthernaClaimTypes.EtherAddress, "0xsomeaddress");

            // Action.
            var result = user.AddClaim(domainClaim);

            // Assert.
            Assert.False(result);
            Assert.DoesNotContain(user.Claims, c => c.Type == EthernaClaimTypes.EtherAddress && c.Value == "0xsomeaddress");
        }

        [Fact]
        public void AddClaim_WithDuplicateClaim_ReturnsFalseAndDoesNotDuplicate()
        {
            // Arrange.
            var user = CreateUser();
            var claim = new UserClaim("custom_type", "custom_value");
            user.AddClaim(claim);

            // Action.
            var result = user.AddClaim(new UserClaim("custom_type", "custom_value"));

            // Assert.
            Assert.False(result);
            Assert.Single(user.Claims, c => c.Type == "custom_type" && c.Value == "custom_value");
        }

        [Fact]
        public void AddLegalAcceptances_WithAcceptances_AppendsRecords()
        {
            // Arrange.
            var user = CreateUser();
            var acceptances = new[]
            {
                new LegalAcceptance(LegalDocumentType.TermsOfService, "2026-06-25", new DateTime(2026, 06, 25, 10, 0, 0, DateTimeKind.Utc)),
                new LegalAcceptance(LegalDocumentType.PrivacyPolicy, "2026-06-25", new DateTime(2026, 06, 25, 10, 0, 0, DateTimeKind.Utc))
            };

            // Action.
            user.AddLegalAcceptances(acceptances);

            // Assert.
            Assert.Equal(acceptances, user.AcceptedLegalDocuments);
        }

        [Fact]
        public void AddLegalAcceptances_CalledTwice_PreservesPreviousHistory()
        {
            // Arrange.
            var user = CreateUser();
            var first = new LegalAcceptance(LegalDocumentType.TermsOfService, "2026-01-01", new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc));
            var second = new LegalAcceptance(LegalDocumentType.TermsOfService, "2026-06-25", new DateTime(2026, 06, 25, 0, 0, 0, DateTimeKind.Utc));
            user.AddLegalAcceptances([first]);

            // Action.
            user.AddLegalAcceptances([second]);

            // Assert.
            Assert.Equal([first, second], user.AcceptedLegalDocuments);
        }

        [Fact]
        public void AddLegalAcceptances_WithNull_ThrowsArgumentNullException()
        {
            // Arrange.
            var user = CreateUser();

            // Assert.
            Assert.Throws<ArgumentNullException>(() => user.AddLegalAcceptances(null!));
        }

        [Fact]
        public void AddRole_WithNewRole_AddsRoleAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            var role = new Role("TestRole");

            // Action.
            var result = user.AddRole(role);

            // Assert.
            Assert.True(result);
            Assert.Contains(user.Roles, r => r.Name == "TestRole");
        }

        [Fact]
        public void AddRole_WithDuplicateRole_ReturnsFalseAndDoesNotDuplicate()
        {
            // Arrange.
            var user = CreateUser();
            var role = new Role("TestRole");
            user.AddRole(role);

            // Action.
            var result = user.AddRole(role);

            // Assert.
            Assert.False(result);
            Assert.Single(user.Roles, r => r.Name == "TestRole");
        }

        [Fact]
        public void ConfirmPhoneNumber_WithPhoneNumber_SetsConfirmedToTrue()
        {
            // Arrange.
            var user = CreateUser();
            user.SetPhoneNumber("123-456-7890");

            // Action.
            user.ConfirmPhoneNumber();

            // Assert.
            Assert.True(user.PhoneNumberConfirmed);
        }

        [Fact]
        public void ConfirmPhoneNumber_WithoutPhoneNumber_ThrowsInvalidOperationException()
        {
            // Arrange.
            var user = CreateUser();

            // Action & Assert.
            Assert.Throws<InvalidOperationException>(() => user.ConfirmPhoneNumber());
        }

        [Fact]
        public void RemoveClaim_WithExistingClaim_RemovesAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            var claim = new UserClaim("custom_type", "custom_value");
            user.AddClaim(claim);

            // Action.
            var result = user.RemoveClaim(claim);

            // Assert.
            Assert.True(result);
            Assert.DoesNotContain(user.Claims, c => c.Type == "custom_type" && c.Value == "custom_value");
        }

        [Fact]
        public void RemoveClaim_WithNonExistingClaim_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            var result = user.RemoveClaim("nonexistent_type", "nonexistent_value");

            // Assert.
            Assert.False(result);
        }

        [Fact]
        public void RemoveEmail_WithEmailSet_ClearsEmailAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            user.SetEmail("test@example.com");

            // Action.
            var result = user.RemoveEmail();

            // Assert.
            Assert.True(result);
            Assert.Null(user.Email);
            Assert.Null(user.NormalizedEmail);
        }

        [Fact]
        public void RemoveEmail_WithNoEmail_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            var result = user.RemoveEmail();

            // Assert.
            Assert.False(result);
        }

        [Fact]
        public void RemoveRole_WithExistingRole_RemovesAndReturnsTrue()
        {
            // Arrange.
            var user = CreateUser();
            var role = new Role("TestRole");
            user.AddRole(role);

            // Action.
            var result = user.RemoveRole("TestRole");

            // Assert.
            Assert.True(result);
            Assert.DoesNotContain(user.Roles, r => r.Name == "TestRole");
        }

        [Fact]
        public void RemoveRole_WithNonExistingRole_ReturnsFalse()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            var result = user.RemoveRole("NonExistingRole");

            // Assert.
            Assert.False(result);
        }

        [Fact]
        public void SetEmail_WithValidEmail_SetsEmailAndNormalizedEmail()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            user.SetEmail("Test@Example.COM");

            // Assert.
            Assert.Equal("Test@Example.COM", user.Email);
            Assert.Equal("TEST@EXAMPLE.COM", user.NormalizedEmail);
        }

        [Fact]
        public void SetEmail_WithInvalidEmail_ThrowsArgumentException()
        {
            // Arrange.
            var user = CreateUser();

            // Action & Assert.
            Assert.Throws<ArgumentException>(() => user.SetEmail("not-an-email"));
        }

        [Fact]
        public void SetPhoneNumber_WithNewNumber_SetsPhoneAndResetsConfirmation()
        {
            // Arrange.
            var user = CreateUser();
            user.SetPhoneNumber("123-456-7890");
            user.ConfirmPhoneNumber();

            // Action.
            user.SetPhoneNumber("098-765-4321");

            // Assert.
            Assert.Equal("098-765-4321", user.PhoneNumber);
            Assert.False(user.PhoneNumberConfirmed);
        }

        [Fact]
        public void SetPhoneNumber_WithNull_ClearsPhoneAndResetsConfirmation()
        {
            // Arrange.
            var user = CreateUser();
            user.SetPhoneNumber("123-456-7890");

            // Action.
            user.SetPhoneNumber(null);

            // Assert.
            Assert.Null(user.PhoneNumber);
            Assert.False(user.PhoneNumberConfirmed);
        }

        [Fact]
        public void SetUsername_WithValidUsername_SetsUsernameAndNormalizedUsername()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            user.SetUsername("newuser");

            // Assert.
            Assert.Equal("newuser", user.Username);
            Assert.Equal("NEWUSER", user.NormalizedUsername);
        }

        [Fact]
        public void SetUsername_WithInvalidUsername_ThrowsArgumentException()
        {
            // Arrange.
            var user = CreateUser();

            // Action & Assert.
            Assert.Throws<ArgumentException>(() => user.SetUsername("ab")); //too short
        }

        [Fact]
        public void SetMaxAllowedClients_WithValidValue_SetsValue()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            user.SetMaxAllowedClients(5);

            // Assert.
            Assert.Equal(5, user.MaxAllowedClients);
        }

        [Fact]
        public void SetMaxAllowedClients_WithZero_SetsToZero()
        {
            // Arrange.
            var user = CreateUser();

            // Action.
            user.SetMaxAllowedClients(0);

            // Assert.
            Assert.Equal(0, user.MaxAllowedClients);
        }

        [Fact]
        public void SetMaxAllowedClients_WithNegativeValue_ThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var user = CreateUser();

            // Action & Assert.
            Assert.Throws<ArgumentOutOfRangeException>(() => user.SetMaxAllowedClients(-1));
        }

        [Fact]
        public void UpdateLastLoginDateTime_WhenCalled_SetsLastLoginDateTimeToUtcNow()
        {
            // Arrange.
            var user = CreateUser();
            var before = DateTime.UtcNow;

            // Action.
            user.UpdateLastLoginDateTime();

            // Assert.
            var after = DateTime.UtcNow;
            Assert.InRange(user.LastLoginDateTime, before, after);
        }
    }
}
