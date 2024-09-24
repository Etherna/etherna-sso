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
using Etherna.SSOServer.Extensions;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Nethereum.Util;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        // Model.
        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = default!;
        }

        // Fields.
        private readonly ILogger<DeletePersonalDataModel> logger;
        private readonly SignInManager<UserBase> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;
        private readonly IWeb3AuthnService web3AuthnService;

        // Constructor.
        public DeletePersonalDataModel(
            ILogger<DeletePersonalDataModel> logger,
            SignInManager<UserBase> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager,
            IWeb3AuthnService web3AuthnService)
        {
            this.logger = logger;
            this.signInManager = signInManager;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
            this.web3AuthnService = web3AuthnService;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;
        public bool IsWeb3User { get; set; }
        public bool RequirePassword { get; set; }
        [TempData]
        public string? StatusMessage { get; set; }

        // Methods.
        public async Task<IActionResult> OnGet()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            IsWeb3User = user is UserWeb3;
            if (!IsWeb3User)
            {
                RequirePassword = await userManager.HasPasswordAsync(user);
            }
            return Page();
        }

        public async Task<IActionResult> OnGetRetriveAuthMessageAsync(string etherAddress) =>
            new JsonResult(await web3AuthnService.RetriveAuthnMessageAsync(etherAddress));

        public async Task<IActionResult> OnGetDeleteWeb3Async(string etherAddress, string signature)
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
            if (await userManager.GetUserAsync(User) is not UserWeb3 user)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!user.EtherAddress.IsTheSameAddress(etherAddress))
            {
                StatusMessage = $"Signing address is different than user's Ethereum address";
                return RedirectToPage();
            }

            // Delete Web3 user.
            await DeleteUserHelperAsync(user);

            return Redirect("~/");
        }

        public async Task<IActionResult> OnPostDeleteWeb2Async()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            IsWeb3User = user is UserWeb3;
            if (IsWeb3User)
                throw new InvalidOperationException();

            RequirePassword = await userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            await DeleteUserHelperAsync(user);

            return Redirect("~/");
        }

        // Private helpers.
        private async Task DeleteUserHelperAsync(UserBase user)
        {
            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{user.Id}'.");

            await signInManager.SignOutAsync();

            logger.AccountDeleted(user.Id, user.Id);
        }
    }
}
