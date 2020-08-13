using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    public class LoggedOutModel : PageModel
    {
        // Properties.
        public string? ClientName { get; set; }
        public string? PostLogoutRedirectUri { get; set; }
        public string? SignOutIFrameUrl { get; set; }

        // Methods.
        public void OnGet(
            string? clientName,
            string? postLogoutRedirectUri,
            string? signOutIFrameUrl)
        {
            ClientName = clientName;
            PostLogoutRedirectUri = postLogoutRedirectUri;
            SignOutIFrameUrl = signOutIFrameUrl;
        }
    }
}
