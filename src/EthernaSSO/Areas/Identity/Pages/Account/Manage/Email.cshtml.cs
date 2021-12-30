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

using Etherna.SSL.Services;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public partial class EmailModel : PageModel
    {
        // Model.
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; } = default!;
        }

        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public EmailModel(
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Properties.
        public string? Email { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await userManager.GetUserIdAsync(user);
                var code = await userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.PageLink(
                    "/Account/ConfirmEmailChange",
                    values: new
                    {
                        userId,
                        email = Input.NewEmail,
                        code
                    });

                var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                    "Views/Emails/ConfirmEmailChange.cshtml",
                    new Services.Views.Emails.ConfirmEmailChangeModel(callbackUrl));

                await emailSender.SendEmailAsync(
                    Input.NewEmail,
                    Services.Views.Emails.ConfirmEmailChangeModel.Title,
                    emailBody);

                StatusMessage = "Confirmation link to change email sent. Please check your email.";
                return RedirectToPage();
            }

            StatusMessage = "Your email is unchanged.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveEmailAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            if (user.Email is null)
            {
                await LoadAsync(user);
                return Page();
            }

            user.RemoveEmail();
            await ssoDbContext.SaveChangesAsync();

            StatusMessage = "Email has been removed";
            return RedirectToPage();
        }

        // Helpers.
        private async Task LoadAsync(UserBase user)
        {
            Email = await userManager.GetEmailAsync(user);
        }
    }
}
