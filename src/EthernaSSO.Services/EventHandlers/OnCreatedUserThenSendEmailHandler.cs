using Digicando.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Settings;
using Etherna.SSOServer.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnCreatedUserThenSendEmailHandler : EventHandlerBase<EntityCreatedEvent<User>>
    {
        // Fields.
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IEmailService emailService;
        private readonly MVCSettings options;
        private readonly UserManager<User> userManager;

        // Constructors.
        public OnCreatedUserThenSendEmailHandler(
            IHttpContextAccessor contextAccessor,
            IEmailService emailService,
            IOptions<MVCSettings> options,
            UserManager<User> userManager)
        {
            this.contextAccessor = contextAccessor;
            this.emailService = emailService;
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

            // Send an email with confirmation link.
            var code = await userManager.GenerateEmailConfirmationTokenAsync(@event.Entity);
            var callbackUrl = new UriTemplate(
                $"{contextAccessor.HttpContext.Request.Scheme}://{contextAccessor.HttpContext.Request.Host}" + "{/controller}{/action}{?userId,code}")
                .AddParameters(new
                {
                    controller = options.ConfirmEmailController,
                    action = options.ConfirmEmailAction,
                    userId = @event.Entity.Id,
                    code
                }).Resolve();

            await emailService.SendEmailAsync(@event.Entity.Email, "Confirm your account",
                $"Please confirm your account by clicking this link: <a href=\"{callbackUrl}\">confirm email</a>");
        }
    }
}
