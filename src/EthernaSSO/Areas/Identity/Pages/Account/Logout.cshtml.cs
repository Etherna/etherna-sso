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

using Etherna.SSOServer.Domain.Models;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
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
        private readonly IEventService eventService;
        private readonly IIdentityServerInteractionService idServerInteractService;
        private readonly ILogger<LogoutModel> logger;
        private readonly SignInManager<User> signInManager;

        // Constructor.
        public LogoutModel(
            IEventService eventService,
            IIdentityServerInteractionService idServerInteractService,
            ILogger<LogoutModel> logger,
            SignInManager<User> signInManager)
        {
            this.eventService = eventService;
            this.idServerInteractService = idServerInteractService;
            this.logger = logger;
            this.signInManager = signInManager;
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
            else
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
            var context = await idServerInteractService.GetLogoutContextAsync(LogoutId);

            if (signInManager.IsSignedIn(User))
            {
                await signInManager.SignOutAsync();

                logger.LogInformation("User logged out.");
                await eventService.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
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
