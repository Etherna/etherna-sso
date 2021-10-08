using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class ShowRecoveryCodesModel : PageModel
    {
        // Properties.
        [TempData]
        public IEnumerable<string> RecoveryCodes { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        // Methods.
        public IActionResult OnGet()
        {
            if (RecoveryCodes == null || !RecoveryCodes.Any())
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }

            return Page();
        }
    }
}
