//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
                    HttpContext.Response.Headers["Location"] = "";

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
