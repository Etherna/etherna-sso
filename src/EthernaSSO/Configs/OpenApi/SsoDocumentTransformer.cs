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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.OpenApi
{
    public sealed class SsoDocumentTransformer(string ssoBaseUrl) : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(document);

            // Set info.
            document.Info.Title = "Sso API";
            document.Info.Version = "0.3";
            document.Servers?.Clear();
            document.Tags?.Clear();

            // Enable sso auth.
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["OAuth"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                In = ParameterLocation.Header,
                Name = "Authorization",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{ssoBaseUrl}/connect/authorize", UriKind.Absolute),
                        TokenUrl = new Uri($"{ssoBaseUrl}/connect/token", UriKind.Absolute),
                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"] = "OpenID",
                            ["profile"] = "Profile",
                            ["ether_accounts"] = "Ether accounts",
                            ["role"] = "Role",
                            ["userApi.sso"] = "Sso API"
                        }
                    }
                }
            };

            //important: allow $ref resolution for OpenApiSecuritySchemeReference during serialization.
            document.SetReferenceHostDocument();

            return Task.CompletedTask;
        }
    }
}