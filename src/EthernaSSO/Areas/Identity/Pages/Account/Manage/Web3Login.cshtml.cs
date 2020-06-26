using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class Web3LoginModel : PageModel
    {
        // Fields.
        private readonly UserManager<User> userManager;

        // Constructor.
        public Web3LoginModel(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        // Properties.
        [TempData]
        public string? StatusMessage { get; set; }

        [Display(Name = "Web3 login address")]
        public string? Web3Login { get; private set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            Web3Login = user.EtherLoginAddress;
            return Page();
        }
    }
}
