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

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.OpenApi
{
    /// <summary>
    /// Aligns OpenAPI schema nullability with C# NRT annotations for each property.
    /// <list type="bullet">
    ///   <item>Non-nullable properties: removes the <c>null</c> type that the generator adds by default.</item>
    ///   <item>Nullable properties: ensures <c>null</c> is present, including properties whose schema
    ///   is a bare <c>$ref</c> (e.g. <c>EthAddress?</c>) where the generator omits null.</item>
    /// </list>
    /// Must run after <see cref="SwarmModelsSchemaTransformer"/>.
    /// </summary>
    public sealed class NullableReferenceTypesSchemaTransformer : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(
            OpenApiSchema schema,
            OpenApiSchemaTransformerContext context,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(context);

            if (context.JsonPropertyInfo?.AttributeProvider is not PropertyInfo propertyInfo)
                return Task.CompletedTask;

            var nullabilityInfo = new NullabilityInfoContext().Create(propertyInfo);
            if (nullabilityInfo.ReadState is not NullabilityState.Nullable)
                schema.Type &= ~JsonSchemaType.Null;
            // Nullable $ref schemas (e.g. EthAddress?) are fixed by NullableStructDocumentTransformer.

            return Task.CompletedTask;
        }
    }
}
