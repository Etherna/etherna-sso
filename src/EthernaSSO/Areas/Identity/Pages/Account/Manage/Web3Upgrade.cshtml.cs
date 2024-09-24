// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

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

#pragma warning disable IDE0060 // Remove unused parameter
        public Task<IActionResult> OnGetUpgradeWeb3Async(string etherAddress, string signature)
#pragma warning restore IDE0060 // Remove unused parameter
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
