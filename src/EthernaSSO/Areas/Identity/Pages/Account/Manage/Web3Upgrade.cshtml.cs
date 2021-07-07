using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nethereum.Util;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class Web3UpgradeModel : PageModel
    {
        // Fields.
        private readonly SignInManager<UserBase> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;
        private readonly IWeb3AuthnService web3AuthnService;

        // Constructor.
        public Web3UpgradeModel(
            SignInManager<UserBase> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager,
            IWeb3AuthnService web3AuthnService)
        {
            this.signInManager = signInManager;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
            this.web3AuthnService = web3AuthnService;
        }

        // Properties.
        public bool RequirePassword { get; set; }
        [TempData]
        public string? StatusMessage { get; set; }

        // Methods.
        public void OnGet()
        { }

        public async Task<IActionResult> OnGetRetriveAuthMessageAsync(string etherAddress) =>
            new JsonResult(await web3AuthnService.RetriveAuthnMessageAsync(etherAddress));

        public async Task<IActionResult> OnGetUpgradeWeb3Async(string etherAddress, string signature)
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

            //delete used token
            await ssoDbContext.Web3LoginTokens.DeleteAsync(token);

            //verify address
            if (await userManager.GetUserAsync(User) is not UserWeb2 userWeb2)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!userWeb2.EtherLoginAddress.IsTheSameAddress(etherAddress))
            {
                StatusMessage = $"Signing address is different than user's Ethereum login address";
                return RedirectToPage();
            }

            // Upgrade user to Web3 account.
            var userWeb3 = new UserWeb3(userWeb2);

            //deleting and recreating because of this https://etherna.atlassian.net/browse/MODM-83
            //await ssoDbContext.Users.ReplaceAsync(userWeb3);
            await ssoDbContext.Users.DeleteAsync(userWeb2);
            await ssoDbContext.Users.CreateAsync(userWeb3);

            //refresh cookie claims
            await signInManager.RefreshSignInAsync(userWeb3);

            return RedirectToPage("./Index");
        }
    }
}
