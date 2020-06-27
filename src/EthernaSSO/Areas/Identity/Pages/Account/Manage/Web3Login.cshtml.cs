using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Domain;
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
        private readonly SignInManager<User> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<User> userManager;
        private readonly IWeb3AuthnService web3AuthnService;

        // Constructor.
        public Web3LoginModel(
            SignInManager<User> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<User> userManager,
            IWeb3AuthnService web3AuthnService)
        {
            this.signInManager = signInManager;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
            this.web3AuthnService = web3AuthnService;
        }

        // Properties.
        public bool ShowRemoveButton { get; set; }

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

            ShowRemoveButton = user.CanRemoveEtherLoginAddress;
            Web3Login = user.EtherLoginAddress;

            return Page();
        }

        public async Task<IActionResult> OnGetRetriveAuthMessageAsync(string etherAddress) =>
            new JsonResult(await web3AuthnService.RetriveAuthnMessageAsync(etherAddress));

        public async Task<IActionResult> OnGetConfirmSignature(string etherAddress, string signature)
        {
            // Verify signature.
            //get token
            var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            if (token is null)
            {
                StatusMessage = $"Web3 authentication code for {etherAddress} address not found";
                return RedirectToPage();
            }

            //check signature
            var verifiedSignature = web3AuthnService.VerifySignature(token.Code, etherAddress, signature);

            if (!verifiedSignature)
            {
                StatusMessage = $"Invalid signature for web3 authentication";
                return RedirectToPage();
            }

            // Add web3 login to user.
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            user.SetEtherLoginAddress(etherAddress);
            await ssoDbContext.SaveChangesAsync();

            // Delete used token.
            await ssoDbContext.Web3LoginTokens.DeleteAsync(token);

            StatusMessage = $"Web3 login has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            user.RemoveEtherLoginAddress();
            await ssoDbContext.SaveChangesAsync();

            await signInManager.RefreshSignInAsync(user);
            StatusMessage = "Web3 login was removed.";

            return RedirectToPage();
        }
    }
}
