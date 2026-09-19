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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.SSOServer.Areas.Api
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class ErrorResultsTest
    {
        // Tests.
        [Fact]
        public async Task GetErrorResult_WritesTheMessageAsJsonString()
        {
            // Arrange.
            await using var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            using var responseBody = new MemoryStream();
            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
            httpContext.Response.Body = responseBody;

            // Action.
            await ErrorResults.GetErrorResult(StatusCodes.Status404NotFound, "User not found").ExecuteAsync(httpContext);

            // Assert.
            responseBody.Position = 0;
            using var body = await JsonDocument.ParseAsync(responseBody);
            Assert.Equal(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", httpContext.Response.ContentType);
            Assert.Equal(JsonValueKind.String, body.RootElement.ValueKind);
            Assert.Equal("User not found", body.RootElement.GetString());
        }
    }
}
