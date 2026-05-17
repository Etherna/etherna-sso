// Copyright 2021-present Etherna SA
// This file is part of Etherna ACR.
//
// Etherna ACR is free software: you can redistribute it and/or modify it under the terms of the
// GNU Lesser General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// Etherna ACR is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Etherna ACR.
// If not, see <https://www.gnu.org/licenses/>.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.SSOServer.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class StatusCodeModel : PageModel
    {
        // Properties.
        public int Code { get; private set; }
        public string Description { get; private set; } = "";
        public string Title { get; private set; } = "";

        // Methods.
        public IActionResult OnGet(int code)
        {
            (Code, Title, Description) = code switch
            {
                400 => (400, "Bad Request", "The server could not understand the request due to invalid syntax."),
                401 => (401, "Unauthorized", "Authentication is required to access this resource."),
                403 => (403, "Access Denied", "You don't have permission to access this resource."),
                404 => (404, "Page Not Found", "The page you're looking for doesn't exist or may have been moved."),
                405 => (405, "Method Not Allowed", "The request method is not supported for this resource."),
                408 => (408, "Request Timeout", "The server timed out waiting for the request."),
                429 => (429, "Too Many Requests", "You've sent too many requests. Please wait a moment before trying again."),
                500 => (500, "Internal Server Error", "Something went wrong on our end. Please try again later."),
                502 => (502, "Bad Gateway", "The server received an invalid response from an upstream server."),
                503 => (503, "Service Unavailable", "The service is temporarily unavailable. Please try again later."),
                _ => (code, "Unexpected Error", "Something went wrong while processing your request.")
            };

            Response.StatusCode = Code;
            return Page();
        }
    }
}
