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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Etherna.SSOServer.Configs.Validation
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class MustBeTrueAttributeTest
    {
        [Fact]
        public void IsValid_WithTrue_ReturnsTrue()
        {
            // Assert.
            Assert.True(new MustBeTrueAttribute().IsValid(true));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(null)]
        public void IsValid_WithNotTrue_ReturnsFalse(object? value)
        {
            // Assert.
            Assert.False(new MustBeTrueAttribute().IsValid(value));
        }

        [Fact]
        public void AddValidation_EmitsDedicatedClientRule()
        {
            // The built-in unobtrusive "required" rule skips checkboxes, so the attribute must emit its
            // own "mustbetrue" rule (matched by the adapter registered in Static/js/site.js) for the
            // checkbox to be validated client-side.

            // Arrange.
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(typeof(bool));
            var attributes = new Dictionary<string, string>();
            var context = new ClientModelValidationContext(new ActionContext(), metadata, metadataProvider, attributes);
            var attribute = new MustBeTrueAttribute { ErrorMessage = "You must accept." };

            // Action.
            attribute.AddValidation(context);

            // Assert.
            Assert.Equal("true", attributes["data-val"]);
            Assert.Equal("You must accept.", attributes[$"data-val-{MustBeTrueAttribute.RuleName}"]);
        }
    }
}
