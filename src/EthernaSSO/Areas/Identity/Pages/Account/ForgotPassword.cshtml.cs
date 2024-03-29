// Copyright 2021-present Etherna Sa
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
