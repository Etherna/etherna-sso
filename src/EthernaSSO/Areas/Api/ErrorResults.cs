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
using Microsoft.AspNetCore.Mvc;

namespace Etherna.SSOServer.Areas.Api
{
    public static class ErrorResults
    {
        // Methods.
        public static IResult GetBadRequestErrorResult(
            string? customMessage = null) =>
            GetErrorResult(StatusCodes.Status400BadRequest, customMessage ?? "Bad request");

        public static IResult GetErrorResult(
            int statusCode,
            string message) =>
            Results.Json(
                new ObjectResult(message) { StatusCode = statusCode },
                statusCode: statusCode);
        
        public static IResult GetInternalServerErrorResult(
            string? customMessage = null) =>
            GetErrorResult(StatusCodes.Status500InternalServerError, customMessage ?? "Internal server error");

        public static IResult GetLockedErrorResult(
            string? customMessage = null) =>
            GetErrorResult(StatusCodes.Status423Locked, customMessage ?? "Locked");

        public static IResult GetNotFoundErrorResult(
            string? customMessage = null) =>
            GetErrorResult(StatusCodes.Status404NotFound, customMessage ?? "Not found");
        
        public static IResult GetServiceUnavailableErrorResult(
            string? customMessage = null) =>
            GetErrorResult(StatusCodes.Status503ServiceUnavailable, customMessage ?? "Service unavailable");
        
        public static IResult GetUnauthorizedErrorResult(
            string? customMessage = null) =>
            GetErrorResult(StatusCodes.Status401Unauthorized, customMessage ?? "Unauthorized request");
    }
}