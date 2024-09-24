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

using Duende.IdentityServer.Services;
using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        // Models.
        public class InputModel
        {
            public string? LogoutId { get; set; }
            public bool LogoutPromptAccepted { get; set; }
        }

        // Fields.
        private readonly IEventDispatcher eventDispatcher;
        private readonly IIdentityServerInteractionService idServerInteractService;
        private readonly ILogger<LogoutModel> logger;
        private readonly SignInManager<UserBase> signInManager;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public LogoutModel(
            IEventDispatcher eventDispatcher,
            IIdentityServerInteractionService idServerInteractService,
            ILogger<LogoutModel> logger,
            SignInManager<UserBase> signInManager,
            UserManager<UserBase> userManager)
        {
            this.eventDispatcher = eventDispatcher;
            this.idServerInteractService = idServerInteractService;
            this.logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;
        public string? LogoutId { get; set; }

        // Methods.
        public async Task<IActionResult> OnGet(string? logoutId)
        {
            LogoutId = logoutId;

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            var showLogoutPrompt = true;

            if (!signInManager.IsSignedIn(User))
            {
                // if the user is not authenticated, then just show logged out page
                showLogoutPrompt = false;
            }
            else if (logoutId is not null)
            {
                var context = await idServerInteractService.GetLogoutContextAsync(logoutId);
                if (context?.ShowSignoutPrompt == false)
                {
                    // it's safe to automatically sign-out
                    showLogoutPrompt = false;
                }
            }

            if (!showLogoutPrompt)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout();
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            LogoutId = Input.LogoutId;

            if (Input.LogoutPromptAccepted)
            {
                return await Logout();
            }
            return Page();
        }

        // Helpers.
        private async Task<IActionResult> Logout()
        {
            var context = LogoutId is not null ?
                await idServerInteractService.GetLogoutContextAsync(LogoutId) :
                null;

            if (signInManager.IsSignedIn(User))
            {
                await signInManager.SignOutAsync();

                logger.LoggedOut();
                var user = await userManager.GetUserAsync(User);
                await eventDispatcher.DispatchAsync(new UserLogoutSuccessEvent(user));
            }

            return RedirectToPage("LoggedOut", new
            {
                context?.PostLogoutRedirectUri,
                ClientName = context?.ClientName ?? context?.ClientId,
                context?.SignOutIFrameUrl
            });
        }
    }
}
