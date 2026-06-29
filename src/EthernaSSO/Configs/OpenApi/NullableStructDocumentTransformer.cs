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

using Etherna.SSOServer.Areas.Api;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.OpenApi
{
    /// <summary>
    /// Fixes nullable struct properties (e.g. <c>EthAddress?</c>) whose OpenAPI schema is a bare
    /// <c>$ref</c> (<see cref="OpenApiSchemaReference"/>). The <c>$ref</c> serializer ignores any
    /// other fields, so schema transformers cannot add <c>null</c> to such properties.
    /// This document transformer replaces those <c>$ref</c> property schemas with inline nullable
    /// copies (preserving type, format, min/maxLength, pattern).
    /// Must run after all schema transformers.
    /// </summary>
    public sealed class NullableStructDocumentTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(document);

            if (document.Components?.Schemas is null)
                return Task.CompletedTask;

            var schemaNameToType = BuildSchemaNameToTypeMap();
            var nullabilityCtx = new NullabilityInfoContext();

            foreach (var (schemaName, schema) in document.Components.Schemas)
            {
                if (!schemaNameToType.TryGetValue(schemaName, out var csType))
                    continue;

                if (schema.Properties is null) continue;

                Dictionary<string, IOpenApiSchema>? replacements = null;

                foreach (var (propName, propSchema) in schema.Properties)
                {
                    if (propSchema is not OpenApiSchemaReference refSchema)
                        continue;

                    var csProperty = FindCsProperty(csType, propName);
                    if (csProperty is null) continue;

                    // Only nullable structs produce a bare $ref that needs null added.
                    var underlyingType = Nullable.GetUnderlyingType(csProperty.PropertyType);
                    if (underlyingType is null) continue;

                    var nullability = nullabilityCtx.Create(csProperty);
                    if (nullability.ReadState is not NullabilityState.Nullable) continue;

                    // Get the base schema that the $ref points to (e.g. "EthAddress").
                    // Note: refSchema.Id is the JSON Schema $id keyword, not the reference path.
                    // The component name is in refSchema.Reference.Id.
                    var refId = refSchema.Reference?.Id;
                    if (refId is null || !document.Components.Schemas.TryGetValue(refId, out var baseSchema))
                        continue;

                    replacements ??= [];
                    replacements[propName] = new OpenApiSchema
                    {
                        Type = (baseSchema.Type ?? JsonSchemaType.String) | JsonSchemaType.Null,
                        Format = baseSchema.Format,
                        MinLength = baseSchema.MinLength,
                        MaxLength = baseSchema.MaxLength,
                        Pattern = baseSchema.Pattern
                    };
                }

                if (replacements is null) continue;
                foreach (var (name, replacement) in replacements)
                    schema.Properties[name] = replacement;
            }

            return Task.CompletedTask;
        }

        // Build a map from OpenAPI schema name (class simple name) to C# type.
        // Groups are filtered to avoid ambiguity when two types share a simple name.
        private static Dictionary<string, Type> BuildSchemaNameToTypeMap()
        {
            var result = new Dictionary<string, Type>();
            foreach (var group in typeof(SsoApiMarker).Assembly.GetTypes()
                         .GroupBy(t => t.Name))
            {
                using var enumerator = group.GetEnumerator();
                if (!enumerator.MoveNext()) continue;
                var first = enumerator.Current;
                if (enumerator.MoveNext()) continue; // ambiguous — skip
                result[group.Key] = first;
            }
            return result;
        }

        private static PropertyInfo? FindCsProperty(Type type, string jsonPropertyName)
        {
            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var jsonAttr = p.GetCustomAttribute<JsonPropertyNameAttribute>();
                var effectiveName = jsonAttr?.Name
                    ?? JsonNamingPolicy.CamelCase.ConvertName(p.Name);
                if (effectiveName == jsonPropertyName)
                    return p;
            }
            return null;
        }
    }
}
