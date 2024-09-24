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
using Duende.IdentityServer.Stores;
using Etherna.ACR.Helpers;
using Etherna.ACR.Services;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Views.Emails;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    public class SetVerifiedEmailModel : SsoExitPageModelBase
    {
        // Models.
        public class CodeInputModel
        {
            [Required]
            public string Code { get; set; } = default!;
        }
        public class EmailInputModel
        {
            [EmailAddress]
            [Display(Name = "Email (optional)")]
            [Required]
            public string Email { get; set; } = default!;
        }

        // Fields.
        private readonly IEmailSender emailSender;
        private readonly ISsoDbContext context;
        private readonly IIdentityServerInteractionService idServerInteractionService;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly UserManager<UserBase> userManager;

        // Constructors.
        public SetVerifiedEmailModel(
            IEmailSender emailSender,
            IClientStore clientStore,
            ISsoDbContext context,
            IIdentityServerInteractionService idServerInteractionService,
            IRazorViewRenderer razorViewRenderer,
            UserManager<UserBase> userManager)
            : base(clientStore)
        {
            this.emailSender = emailSender;
            this.context = context;
            this.idServerInteractionService = idServerInteractionService;
            this.razorViewRenderer = razorViewRenderer;
            this.userManager = userManager;
        }

        // Properties.
        [BindProperty]
        public CodeInputModel CodeInput { get; set; } = default!;

        [BindProperty]
        public EmailInputModel EmailInput { get; set; } = default!;

        public bool IsCodeSent { get; private set; }
        public bool IsWeb3 { get; private set; }
        public string? ReturnUrl { get; private set; }

        // Methods.
        public async Task OnGetAsync(string? email, bool isCodeSent, string? returnUrl) =>
            await InitializeAsync(email, isCodeSent, returnUrl);

        public async Task<IActionResult> OnGetSkipAsync(string? returnUrl) =>
            await ProceedAsync(returnUrl);

        public async Task<IActionResult> OnPostSendEmailAsync(string? returnUrl)
        {
            // Validate model.
            ModelState.Clear();

            //email validity
            if (!EmailHelper.IsValidEmail(EmailInput.Email))
                ModelState.AddModelError(string.Empty, "Inserted email is not valid.");

            //check for duplicate email
            if (await userManager.FindByEmailAsync(EmailInput.Email) is not null)
                ModelState.AddModelError(string.Empty, "Email already registered.");

            if (ModelState.ErrorCount > 0)
            {
                await InitializeAsync(EmailInput.Email, false, returnUrl);
                return Page();
            }

            // Generate Totp code.
            var user = await userManager.GetUserAsync(User) ?? throw new InvalidOperationException();
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send email.
            var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                "Views/Emails/TotpConfirmEmail.cshtml",
                new TotpConfirmEmailModel(code));

            await emailSender.SendEmailAsync(
                EmailInput.Email,
                TotpConfirmEmailModel.Title,
                emailBody);

            // Return page.
            await InitializeAsync(EmailInput.Email, true, returnUrl);
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmCodeAsync(string email, string? returnUrl)
        {
            // Validate model.
            ModelState.Clear();
            if (string.IsNullOrEmpty(CodeInput.Code))
                ModelState.AddModelError(string.Empty, "Code can't be empty.");

            // Validate code.
            var user = await userManager.GetUserAsync(User) ?? throw new InvalidOperationException();
            var result = await userManager.ConfirmEmailAsync(user, CodeInput.Code);
            if (!result.Succeeded)
                ModelState.AddModelError(string.Empty, "Code is not valid.");

            if (ModelState.ErrorCount > 0)
            {
                await InitializeAsync(email, true, returnUrl);
                return Page();
            }

            // Add email.
            user.SetEmail(email);
            await context.SaveChangesAsync();

            return await ProceedAsync(returnUrl);
        }

        // Helpers.
        private async Task InitializeAsync(string? email, bool isCodeSent, string? returnUrl)
        {
            if (email is not null)
                EmailInput = new EmailInputModel { Email = email };

            var user = await userManager.GetUserAsync(User);
            IsWeb3 = user is UserWeb3;

            IsCodeSent = isCodeSent;
            ReturnUrl = returnUrl;
        }

        private async Task<IActionResult> ProceedAsync(string? returnUrl)
        {
            var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);
            return await ContextedRedirectAsync(context, returnUrl);
        }
    }
}
