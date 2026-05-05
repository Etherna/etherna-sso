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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace Etherna.SSOServer.Middlewares.DebugPages
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseEthernaDebugPages(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            return app.UseMiddleware<DebugPagesMiddleware>();
        }

        public static IApplicationBuilder UseEthernaDebugPages(
            this IApplicationBuilder app,
            DebugPagesOptions options)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(options);

            return app.UseMiddleware<DebugPagesMiddleware>(Options.Create(options));
        }
    }
}