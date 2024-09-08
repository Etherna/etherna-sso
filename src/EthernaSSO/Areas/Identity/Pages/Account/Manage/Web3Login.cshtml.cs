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

using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nethereum.Util;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class Web3LoginModel : PageModel
    {
        // Fields.
        private readonly SignInManager<UserBase> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;
        private readonly IWeb3AuthnService web3AuthnService;

        // Constructor.
        public Web3LoginModel(
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
        [Display(Name = "Ethereum login address")]
        public string? EtherLoginAddress { get; private set; }

        public bool ShowRemoveButton { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            if (await userManager.GetUserAsync(User) is not UserWeb2 user)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            EtherLoginAddress = user.EtherLoginAddress;
            ShowRemoveButton = user.CanRemoveEtherLoginAddress;

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

            //delete used token
            await ssoDbContext.Web3LoginTokens.DeleteAsync(token);

            // Add web3 login to user.
            if (await userManager.GetUserAsync(User) is not UserWeb2 user)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!user.EtherLoginAddress.IsTheSameAddress(etherAddress))
            {
                //check for uniqueness
                var cursor = await ssoDbContext.Users.FindAsync<UserBase>(Builders<UserBase>.Filter.Or(
                    Builders<UserBase>.Filter.Eq(u => u.EtherAddress, etherAddress),    //UserWeb3
                    Builders<UserBase>.Filter.Eq("EtherLoginAddress", etherAddress)));  //UserWeb2
                if (await cursor.AnyAsync())
                {
                    StatusMessage = $"Can't assign Web3 login. It has already been used with another account.";
                    return RedirectToPage();
                }

                //set address
                user.SetEtherLoginAddress(etherAddress);
                await ssoDbContext.SaveChangesAsync();
            }

            StatusMessage = $"Web3 login has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync()
        {
            if (await userManager.GetUserAsync(User) is not UserWeb2 user)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            user.RemoveEtherLoginAddress();
            await ssoDbContext.SaveChangesAsync();

            await signInManager.RefreshSignInAsync(user);
            StatusMessage = "Web3 login was removed.";

            return RedirectToPage();
        }
    }
}
