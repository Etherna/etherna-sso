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

using Etherna.ACR.Services;
using Etherna.SSOServer.Configs;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = default!;
        }

        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public ForgotPasswordModel(
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            UserManager<UserBase> userManager)
        {
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
            this.userManager = userManager;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await userManager.FindByEmailAsync(Input.Email);

            switch (user)
            {
                case null:
                    // Don't reveal that the user does not exist or is not confirmed.
                    return RedirectToPage("./ForgotPasswordConfirmation");
                
                case UserWeb2 _:
                    // Generate url.
                    var code = await userManager.GeneratePasswordResetTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ResetPassword",
                        pageHandler: null,
                        values: new { area = CommonConsts.IdentityArea, code },
                        protocol: Request.Scheme) ?? throw new InvalidOperationException();

                    // Send email.
                    var web2EmailBody = await razorViewRenderer.RenderViewToStringAsync(
                        "Views/Emails/ResetPassword.cshtml",
                        new Services.Views.Emails.ResetPasswordModel(callbackUrl));

                    await emailSender.SendEmailAsync(
                        Input.Email,
                        Services.Views.Emails.ResetPasswordModel.Title,
                        web2EmailBody);

                    return RedirectToPage("./ForgotPasswordConfirmation");
                
                case UserWeb3 userWeb3:
                    // Send email.
                    var web3EmailBody = await razorViewRenderer.RenderViewToStringAsync(
                        "Views/Emails/ResetPasswordWeb3.cshtml",
                        new Services.Views.Emails.ResetPasswordWeb3Model(userWeb3.EtherAddress));

                    await emailSender.SendEmailAsync(
                        Input.Email,
                        Services.Views.Emails.ResetPasswordWeb3Model.Title,
                        web3EmailBody);
                    
                    return RedirectToPage("./ForgotPasswordConfirmation");
                
                default: throw new InvalidOperationException();
            }
        }
    }
}
