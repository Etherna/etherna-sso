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

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Etherna.SSOServer.Configs.Validation
{
    /// <summary>
    /// Requires a boolean value to be true, e.g. a mandatory acceptance checkbox.
    /// Validation runs both server-side and client-side (without a page reload). The built-in
    /// unobtrusive "required" rule intentionally skips checkboxes, so this attribute emits a dedicated
    /// "mustbetrue" rule whose client adapter is registered in <c>Static/js/site.js</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MustBeTrueAttribute : ValidationAttribute, IClientModelValidator
    {
        // Consts.
        //must match the adapter/method name registered in Static/js/site.js
        public const string RuleName = "mustbetrue";

        // Methods.
        public void AddValidation(ClientModelValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, $"data-val-{RuleName}", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        }

        public override bool IsValid(object? value) => value is true;

        // Helpers.
        private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (!attributes.ContainsKey(key))
                attributes.Add(key, value);
        }
    }
}
