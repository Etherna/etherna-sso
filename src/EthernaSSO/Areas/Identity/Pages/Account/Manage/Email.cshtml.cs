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
using Etherna.SSOServer.Models;
using Etherna.SSOServer.Pages;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel(
        IEmailSender emailSender,
        INewsletterService newsletterService,
        IRazorViewRenderer razorViewRenderer,
        ISsoDbContext ssoDbContext,
        UserManager<UserBase> userManager)
        : StatusMessagePageModel
    {
        // Model.
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; } = null!;
        }

        // Properties.
        public string? Email { get; set; }

        public bool ShowNewsletterSection { get; set; }
        public bool SubscribedToNewsletter { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

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
                    }) ?? throw new InvalidOperationException();

                var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                    "Views/Emails/ConfirmEmailChange.cshtml",
                    new Services.Views.Emails.ConfirmEmailChangeModel(callbackUrl));

                await emailSender.SendEmailAsync(
                    Input.NewEmail,
                    Services.Views.Emails.ConfirmEmailChangeModel.Title,
                    emailBody);

                StatusMessage = new StatusMessage("Confirmation link to change email sent. Please check your email.");
                return RedirectToPage();
            }

            StatusMessage = new StatusMessage("Your email is unchanged.", StatusMessageType.Warning);
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

            StatusMessage = new StatusMessage("Email has been removed");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSubscribeNewsletterAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            var email = await userManager.GetEmailAsync(user);
            if (email is null)
            {
                StatusMessage = new StatusMessage("You must add a verified email before subscribing.", StatusMessageType.Warning);
                return RedirectToPage();
            }

            // Idempotent: the newsletter service performs an add-or-update on the allowed list, so a repeated
            // call (or a double submit) is harmless.
            await newsletterService.SubscribeEmailAsync(email, NewsletterSubscriptionSource.AccountSettings);

            StatusMessage = new StatusMessage("You have been subscribed to our newsletter.");
            return RedirectToPage();
        }

        // Helpers.
        private async Task LoadAsync(UserBase user)
        {
            Email = await userManager.GetEmailAsync(user);

            // Offer the newsletter section whenever the user has a (verified) email, regardless of whether the
            // newsletter service is enabled: when disabled, IsSubscribedAsync reports "not subscribed" and the
            // subscribe call is a no-op. The button is shown only when the contact is not already subscribed.
            ShowNewsletterSection = Email is not null;
            SubscribedToNewsletter = ShowNewsletterSection && await newsletterService.IsSubscribedAsync(Email!);
        }
    }
}
