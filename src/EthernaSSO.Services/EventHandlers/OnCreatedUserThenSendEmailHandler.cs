using Digicando.DomainEvents;
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
    class OnCreatedUserThenSendEmailHandler : EventHandlerBase<EntityCreatedEvent<User>>
    {
        // Fields.
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IEmailSender emailSender;
        private readonly PageSettings options;
        private readonly UserManager<User> userManager;

        // Constructors.
        public OnCreatedUserThenSendEmailHandler(
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
