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
using Etherna.SSOServer.Services.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnCreatedUserThenSendConfirmationEmailHandler : EventHandlerBase<EntityCreatedEvent<User>>
    {
        // Fields.
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IEmailSender emailSender;
        private readonly PageSettings options;
        private readonly UserManager<User> userManager;

        // Constructors.
        public OnCreatedUserThenSendConfirmationEmailHandler(
            IHttpContextAccessor contextAccessor,
            IEmailSender emailService,
            IOptions<PageSettings> options,
            UserManager<User> userManager)
        {
            this.contextAccessor = contextAccessor;
            this.emailSender = emailService;
            this.options = options.Value;
            this.userManager = userManager;
        }

        // Methods.
        public override async Task HandleAsync(EntityCreatedEvent<User> @event)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            if (@event.Entity.Email is null)
                return;
            if (contextAccessor.HttpContext is null)
                throw new InvalidOperationException();

            var user = @event.Entity;

            // Send an email with confirmation link.
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = new UriTemplate(
                $"{contextAccessor.HttpContext.Request.Scheme}://{contextAccessor.HttpContext.Request.Host}" + "{/area}" + options.ConfirmEmailPageUrl + "{?userId,code}")
                .AddParameters(new
                {
                    area = options.ConfirmEmailPageArea,
                    userId = @event.Entity.Id,
                    code
                }).Resolve();

            await emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }
    }
}
