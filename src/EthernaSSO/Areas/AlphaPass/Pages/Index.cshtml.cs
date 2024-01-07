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

using Etherna.ACR.Helpers;
using Etherna.ACR.Services;
using Etherna.SSOServer.Configs;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.AlphaPass.Pages
{
    public class IndexModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = default!;
        }

        // Fields.
        private readonly ISsoDbContext dbContext;
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;

        // Constructor.
        public IndexModel(
            ISsoDbContext dbContext,
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer)
        {
            this.dbContext = dbContext;
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        // Methods.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var normalizedEmail = EmailHelper.NormalizeEmail(Input.Email);

            // Verify if already in queue.
            AlphaPassRequest request;
            var prevRequest = await dbContext.AlphaPassRequests.TryFindOneAsync(r => r.NormalizedEmail == normalizedEmail);
            if (prevRequest != null)
            {
                //if already enqueued, return error
                if (prevRequest.IsEmailConfirmed)
                {
                    StatusMessage = "An Alpha Pass has already been requested with this email";
                    return Page();
                }

                //take the old one as valid request, and send again
                request = prevRequest;
            }
            else
            {
                //create a new one
                request = new AlphaPassRequest(Input.Email);
                await dbContext.AlphaPassRequests.CreateAsync(request);
            }

            // Generate url.
            var callbackUrl = Url.Page(
                "/ConfirmRequest",
                pageHandler: null,
                values: new
                {
                    area = CommonConsts.AlphaPassArea,
                    email = normalizedEmail.ToLowerInvariant(),
                    secret = request.Secret
                },
                protocol: Request.Scheme) ?? throw new InvalidOperationException();

            // Send email.
            var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                "Views/Emails/AlphaPassRequestEmailConfirmation.cshtml",
                new Services.Views.Emails.AlphaPassRequestEmailConfirmationModel(callbackUrl));

            await emailSender.SendEmailAsync(
                Input.Email,
                Services.Views.Emails.AlphaPassRequestEmailConfirmationModel.Title,
                emailBody);

            // Confirm.
            StatusMessage = "A confirmation message has been sent to you. Please verify your email";
            return Page();
        }
    }
}
