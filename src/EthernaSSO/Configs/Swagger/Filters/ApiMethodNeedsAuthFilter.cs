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

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.SSOServer.Configs.Swagger.Filters
{
    public class ApiMethodNeedsAuthFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(operation, nameof(operation));
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var methodAttributes = context.MethodInfo.GetCustomAttributes(true);
            
            // Check if allow anonymous.
            if (methodAttributes.OfType<AllowAnonymousAttribute>().Any() ||
                (context.MethodInfo.DeclaringType != null &&
                 context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()))
                return;
            
            // Otherwise, require authentication by default.
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {{
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Id = "OAuth", Type = ReferenceType.SecurityScheme }
                    },
                    new List<string>()
                }}
            ];
        }
    }
}