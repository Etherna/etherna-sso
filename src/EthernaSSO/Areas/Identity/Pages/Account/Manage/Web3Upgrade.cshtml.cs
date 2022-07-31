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
        //private readonly SignInManager<UserBase> signInManager;
        //private readonly ISsoDbContext ssoDbContext;
        //private readonly UserManager<UserBase> userManager;
        //private readonly IUserService userService;
        private readonly IWeb3AuthnService web3AuthnService;

        // Constructor.
        public Web3UpgradeModel(
            //SignInManager<UserBase> signInManager,
            //ISsoDbContext ssoDbContext,
            //UserManager<UserBase> userManager,
            //IUserService userService,
            IWeb3AuthnService web3AuthnService)
        {
            //this.signInManager = signInManager;
            //this.ssoDbContext = ssoDbContext;
            //this.userManager = userManager;
            //this.userService = userService;
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

        public Task<IActionResult> OnGetUpgradeWeb3Async(string etherAddress, string signature)
        {
            // Temporary disabled (see: https://etherna.atlassian.net/browse/ESSO-165)
            StatusMessage = $"Error, this function is temporary disabled";
            return Task.FromResult<IActionResult>(RedirectToPage());

            //// Verify signature.
            ////get token
            //var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            //if (token is null)
            //{
            //    StatusMessage = $"Web3 authentication code for {etherAddress} address not found";
            //    return RedirectToPage();
            //}
            
            ////check signature
            //var verifiedSignature = web3AuthnService.VerifySignature(token.Code, etherAddress, signature);
            //if (!verifiedSignature)
            //{
            //    StatusMessage = $"Invalid signature for web3 authentication";
            //    return RedirectToPage();
            //}

            ////delete used token
            //await ssoDbContext.Web3LoginTokens.DeleteAsync(token);

            ////verify address
            //if (await userManager.GetUserAsync(User) is not UserWeb2 userWeb2)
            //    return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            //if (!userWeb2.EtherLoginAddress.IsTheSameAddress(etherAddress))
            //{
            //    StatusMessage = $"Signing address is different than user's Ethereum login address";
            //    return RedirectToPage();
            //}

            //// Upgrade user to Web3 account.
            //var userWeb3 = await userService.UpgradeToWeb3(userWeb2);

            ////refresh cookie claims
            //await signInManager.RefreshSignInAsync(userWeb3);

            //return RedirectToPage("./Index");
        }
    }
}
