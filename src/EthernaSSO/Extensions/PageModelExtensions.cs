using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Etherna.SSOServer.Extensions
{
    public static class PageModelExtensions
    {
        public static IActionResult LoadingPage(this PageModel page, string pageName, string redirectUrl)
        {
            if (page is null)
                throw new ArgumentNullException(nameof(page));

            page.HttpContext.Response.StatusCode = 200;
            page.HttpContext.Response.Headers["Location"] = "";

            return page.RedirectToPage(pageName, new { redirectUrl });
        }
    }
}
