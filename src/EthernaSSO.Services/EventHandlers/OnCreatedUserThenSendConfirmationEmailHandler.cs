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

using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.RCL.Views.Emails;
using Etherna.SSOServer.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnCreatedUserThenSendConfirmationEmailHandler : EventHandlerBase<EntityCreatedEvent<UserBase>>
    {
        // Fields.
        private readonly IEmailSender emailService;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly IUrlHelper urlHelper;
        private readonly UserManager<UserBase> userManager;

        // Constructors.
        public OnCreatedUserThenSendConfirmationEmailHandler(
            IActionContextAccessor actionContextAccessor,
            IEmailSender emailService,
            IRazorViewRenderer razorViewRenderer,
            IUrlHelperFactory urlHelperFactory,
            UserManager<UserBase> userManager)
        {
            this.emailService = emailService;
            this.razorViewRenderer = razorViewRenderer;
            urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            this.userManager = userManager;
        }

        // Methods.
        public override async Task HandleAsync(EntityCreatedEvent<UserBase> @event)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            if (@event.Entity.Email is null)
                return;

            var user = @event.Entity;

            // Send an email with confirmation link.
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = urlHelper.PageLink(
                "/Account/ConfirmEmail",
                values: new
                {
                    area = "Identity",
                    userId = @event.Entity.Id,
                    code
                });

            var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                "Views/Emails/ConfirmEmail.cshtml",
                new ConfirmEmailModel(callbackUrl));

            await emailService.SendEmailAsync(
                user.Email,
                ConfirmEmailModel.Title,
                emailBody);
        }
    }
}
