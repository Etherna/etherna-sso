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

using Etherna.SSOServer.Configs;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Models;
using Etherna.SSOServer.Pages;
using Etherna.SSOServer.Services.Domain;
using Etherna.SSOServer.Services.Views.Emails;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.AlphaPass.Pages
{
    public class IndexModel(
        ISsoDbContext dbContext,
        IEmailSender emailSender,
        IRazorViewRenderer razorViewRenderer)
        : StatusMessagePageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = null!;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = null!;

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
                    StatusMessage = new StatusMessage("An Alpha Pass has already been requested with this email", StatusMessageType.Warning);
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
                new AlphaPassRequestEmailConfirmationModel(callbackUrl));

            await emailSender.SendEmailAsync(
                Input.Email,
                AlphaPassRequestEmailConfirmationModel.Title,
                emailBody);

            // Confirm.
            StatusMessage = new StatusMessage("A confirmation message has been sent to you. Please verify your email");
            return Page();
        }
    }
}
