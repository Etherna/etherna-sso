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

using Etherna.SwarmSdk.JsonConverters;
using Etherna.SwarmSdk.Models;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.OpenApi
{
    public sealed class SwarmModelsSchemaTransformer(
        NumericFormat bzzFormat = NumericFormat.AsString,
        bool dateTimeOffsetAsString = true,
        NumericFormat xdaiFormat = NumericFormat.AsString)
        : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(
            OpenApiSchema schema,
            OpenApiSchemaTransformerContext context,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(context);

            // For nullable struct types (e.g. EthAddress?), ensure the Null flag is always set.
            // For reference types, preserve whatever null flag the generator already assigned.
            // Use (schema.Type ?? 0) to avoid null-propagation via lifted bitwise operators.
            var isNullableStruct = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) != null;
            var nullFlag = isNullableStruct ? JsonSchemaType.Null : ((schema.Type ?? (JsonSchemaType)0) & JsonSchemaType.Null);

            if (context.JsonTypeInfo.Type == typeof(BzzValue) || context.JsonTypeInfo.Type == typeof(BzzValue?))
            {
                switch (bzzFormat)
                {
                    case NumericFormat.AsFloat:
                        schema.Type = JsonSchemaType.Number | nullFlag;
                        schema.Format = "double";
                        break;
                    case NumericFormat.AsInteger:
                        schema.Type = JsonSchemaType.Integer | nullFlag;
                        schema.Format = "int64";
                        break;
                    case NumericFormat.AsString:
                        schema.Type = JsonSchemaType.String | nullFlag;
                        schema.Format = null;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            if (context.JsonTypeInfo.Type == typeof(DateTimeOffset) || context.JsonTypeInfo.Type == typeof(DateTimeOffset?))
            {
                if (dateTimeOffsetAsString)
                {
                    schema.Type = JsonSchemaType.String | nullFlag;
                    schema.Format = "date-time";
                }
                else
                {
                    schema.Type = JsonSchemaType.Integer | nullFlag;
                    schema.Format = "int64";
                }
            }
            if (context.JsonTypeInfo.Type == typeof(EthAddress) || context.JsonTypeInfo.Type == typeof(EthAddress?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = EthAddress.AddressSize * 2;
                schema.MaxLength = EthAddress.AddressSize * 2 + 2;
                schema.Pattern = $"^(0x)?[a-fA-F0-9]{{{EthAddress.AddressSize * 2}}}$";
            }
            if (typeof(IEnumerable<EthAddress>).IsAssignableFrom(context.JsonTypeInfo.Type))
            {
                schema.Items = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    MinLength = EthAddress.AddressSize * 2,
                    MaxLength = EthAddress.AddressSize * 2 + 2,
                    Pattern = $"^(0x)?[a-fA-F0-9]{{{EthAddress.AddressSize * 2}}}$"
                };
            }
            if (context.JsonTypeInfo.Type == typeof(EthTxHash) || context.JsonTypeInfo.Type == typeof(EthTxHash?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = EthTxHash.HashSize * 2;
                schema.MaxLength = EthTxHash.HashSize * 2;
                schema.Pattern = $"^[a-fA-F0-9]{{{EthTxHash.HashSize * 2}}}$";
            }
            if (context.JsonTypeInfo.Type == typeof(PostageBatchId) || context.JsonTypeInfo.Type == typeof(PostageBatchId?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = PostageBatchId.BatchIdSize * 2;
                schema.MaxLength = PostageBatchId.BatchIdSize * 2;
                schema.Pattern = $"^[a-fA-F0-9]{{{PostageBatchId.BatchIdSize * 2}}}$";
            }
            if (context.JsonTypeInfo.Type == typeof(PostageStamp) || context.JsonTypeInfo.Type == typeof(PostageStamp?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = PostageStamp.StampSize * 2;
                schema.MaxLength = PostageStamp.StampSize * 2;
                schema.Pattern = $"^[a-fA-F0-9]{{{PostageStamp.StampSize * 2}}}$";
                schema.Properties?.Clear();
            }
            if (context.JsonTypeInfo.Type == typeof(SwarmAddress) || context.JsonTypeInfo.Type == typeof(SwarmAddress?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = SwarmReference.PlainSize * 2;
                schema.Pattern = $"^[a-fA-F0-9]{{{SwarmReference.PlainSize * 2}}}.*$";
                schema.Properties?.Clear();
            }
            if (context.JsonTypeInfo.Type == typeof(SwarmFeedTopic) || context.JsonTypeInfo.Type == typeof(SwarmFeedTopic?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = SwarmFeedTopic.TopicSize * 2;
                schema.MaxLength = SwarmFeedTopic.TopicSize * 2;
                schema.Pattern = $"^[a-fA-F0-9]{{{SwarmFeedTopic.TopicSize * 2}}}$";
            }
            if (context.JsonTypeInfo.Type == typeof(SwarmHash) || context.JsonTypeInfo.Type == typeof(SwarmHash?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = SwarmHash.HashSize * 2;
                schema.MaxLength = SwarmHash.HashSize * 2;
                schema.Pattern = $"^[a-fA-F0-9]{{{SwarmHash.HashSize * 2}}}$";
            }
            if (context.JsonTypeInfo.Type == typeof(SwarmOverlayAddress) || context.JsonTypeInfo.Type == typeof(SwarmOverlayAddress?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = SwarmOverlayAddress.AddressSize * 2;
                schema.MaxLength = SwarmOverlayAddress.AddressSize * 2;
                schema.Pattern = $"^[a-fA-F0-9]{{{SwarmOverlayAddress.AddressSize * 2}}}$";
            }
            if (context.JsonTypeInfo.Type == typeof(SwarmReference) || context.JsonTypeInfo.Type == typeof(SwarmReference?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = SwarmReference.PlainSize * 2;
                schema.MaxLength = SwarmReference.EncryptedSize * 2;
                schema.Pattern = $"^([a-fA-F0-9]{{{SwarmReference.PlainSize * 2}}}|[a-fA-F0-9]{{{SwarmReference.EncryptedSize * 2}}})$";
                schema.Properties?.Clear();
            }
            if (context.JsonTypeInfo.Type == typeof(SwarmSocIdentifier) || context.JsonTypeInfo.Type == typeof(SwarmSocIdentifier?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = SwarmSocIdentifier.IdentifierSize * 2;
                schema.MaxLength = SwarmSocIdentifier.IdentifierSize * 2;
                schema.Pattern = $"^[a-fA-F0-9]{{{SwarmSocIdentifier.IdentifierSize * 2}}}$";
            }
            if (context.JsonTypeInfo.Type == typeof(SwarmSocSignature) || context.JsonTypeInfo.Type == typeof(SwarmSocSignature?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
                schema.MinLength = SwarmSocSignature.SignatureSize * 2;
                schema.MaxLength = SwarmSocSignature.SignatureSize * 2;
                schema.Pattern = $"^[a-fA-F0-9]{{{SwarmSocSignature.SignatureSize * 2}}}$";
            }
            if (context.JsonTypeInfo.Type == typeof(SwarmUri) || context.JsonTypeInfo.Type == typeof(SwarmUri?))
            {
                schema.Type = JsonSchemaType.String | nullFlag;
                schema.Format = null;
            }
            if (context.JsonTypeInfo.Type == typeof(TagId) || context.JsonTypeInfo.Type == typeof(TagId?))
            {
                schema.Type = JsonSchemaType.Integer | nullFlag;
                schema.Format = "int64";
            }
            if (context.JsonTypeInfo.Type == typeof(TimeSpan) || context.JsonTypeInfo.Type == typeof(TimeSpan?))
            {
                schema.Type = JsonSchemaType.Integer | nullFlag;
                schema.Format = "int64";
                schema.Pattern = null;
            }
            if (context.JsonTypeInfo.Type == typeof(XDaiValue) || context.JsonTypeInfo.Type == typeof(XDaiValue?))
            {
                switch (xdaiFormat)
                {
                    case NumericFormat.AsFloat:
                        schema.Type = JsonSchemaType.Number | nullFlag;
                        schema.Format = "double";
                        break;
                    case NumericFormat.AsInteger:
                        schema.Type = JsonSchemaType.Integer | nullFlag;
                        schema.Format = "int64";
                        break;
                    case NumericFormat.AsString:
                        schema.Type = JsonSchemaType.String | nullFlag;
                        schema.Format = null;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return Task.CompletedTask;
        }
    }
}
