using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserProfileModel : PageModel
    {
        public string? Id { get; private set; } = default!;

        public void OnGet(string? id)
        {
            Id = id;
        }
    }
}
