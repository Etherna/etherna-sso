using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Etherna.SSOServer.Pages
{
    public class RedirectModel : PageModel
    {
        // Methods.
        public string RedirectUrl { get; private set; } = default!;

        // Properties.
        public void OnGet(string redirectUrl)
        {
            if (redirectUrl is null)
                throw new ArgumentNullException(nameof(redirectUrl));

            RedirectUrl = redirectUrl;
        }
    }
}
