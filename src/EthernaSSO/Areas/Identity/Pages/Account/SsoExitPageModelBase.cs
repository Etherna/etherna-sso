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

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Etherna.SSOServer.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    public abstract class SsoExitPageModelBase : PageModel
    {
        // Fields.
        private readonly IClientStore clientStore;

        // Constructor.
        protected SsoExitPageModelBase(IClientStore clientStore)
        {
            this.clientStore = clientStore;
        }

        // Methods.
        protected async Task<IActionResult> ContextedRedirectAsync(
            AuthorizationRequest? context,
            string? returnUrl)
        {
            returnUrl ??= Url.Content("~/");
            if (string.IsNullOrEmpty(returnUrl))
                throw new ArgumentException($"'{nameof(returnUrl)}' cannot be empty.", nameof(returnUrl));

            // Identify redirect.
            if (context?.Client != null)
            {
                if (await clientStore.IsPkceClientAsync(context.Client.ClientId))
                {
                    //if the client is PKCE then we assume it's native, so this change in how to
                    //return the response is for better UX for the end user
                    HttpContext.Response.StatusCode = 200;
                    HttpContext.Response.Headers.Location = "";

                    return RedirectToPage("/Redirect", new { redirectUrl = returnUrl });
                }

                //we can trust returnUrl since GetAuthorizationContextAsync returned non-null
                return Redirect(returnUrl);
            }

            //request for a local page, otherwise user might have clicked on a malicious link - should be logged
            return LocalRedirect(returnUrl);
        }
    }
}
