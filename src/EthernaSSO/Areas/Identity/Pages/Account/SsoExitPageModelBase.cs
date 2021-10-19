using Etherna.SSOServer.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
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
