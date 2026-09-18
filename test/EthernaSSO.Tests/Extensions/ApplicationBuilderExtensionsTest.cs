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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.SSOServer.Extensions
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class ApplicationBuilderExtensionsTest
    {
        // Consts.
        private const string StatusPageBody = "status page";

        // Tests.
        [Theory]
        [InlineData("/Identity/Account/Manage", null, true)]
        [InlineData("/Identity/Account/Manage", "Bearer token", false)]
        [InlineData("/api/v0.3/identity", null, false)]
        [InlineData("/connect/userinfo", null, false)]
        [InlineData("/.well-known/openid-configuration", null, false)]
        public async Task UseStatusCodePagesOnPageRequests_WithForbiddenResponse_RendersTheStatusPageOnlyOnPageRequests(
            string path,
            string? authorization,
            bool expectedStatusPage)
        {
            // Arrange.
            using var host = await new HostBuilder()
                .ConfigureWebHost(webHost => webHost
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseStatusCodePagesOnPageRequests("/StatusCode", "?code={0}");
                        app.Run(async context =>
                        {
                            if (context.Request.Path == "/StatusCode")
                                await context.Response.WriteAsync(StatusPageBody);
                            else
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        });
                    }))
                .StartAsync();
            using var client = host.GetTestClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(path, UriKind.Relative));
            if (authorization is not null)
                request.Headers.TryAddWithoutValidation("Authorization", authorization);

            // Action.
            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert.
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(expectedStatusPage ? StatusPageBody : "", body);
        }
    }
}
