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
            var userId = await userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");

            await signInManager.SignOutAsync();

            logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);
        }
    }
}
