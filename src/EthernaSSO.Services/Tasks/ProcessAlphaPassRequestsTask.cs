using Etherna.ACR.Services;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Views.Emails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public class ProcessAlphaPassRequestsTask : IProcessAlphaPassRequestsTask
    {
        // Consts.
        private readonly TimeSpan DefaultInvitationDuration = TimeSpan.FromDays(30);
        private const int MaxRequestsPerTime = 1;

        public const string TaskId = "processAlphaPassRequestsTask";

        // Fields.
        private readonly ISsoDbContext dbContext;
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly IServiceProvider serviceProvider;

        // Constructor.
        public ProcessAlphaPassRequestsTask(
            ISsoDbContext dbContext,
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
            this.serviceProvider = serviceProvider;
        }

        // Methods.
        public async Task RunAsync()
        {
            // Create an action context. Required for view rendering.
            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            // Get requests.
            var requests = await dbContext.AlphaPassRequests.QueryElementsAsync(elements =>
                elements.Where(e => e.IsEmailConfirmed)
                        .Where(e => !e.IsInvitationSent)
                        .OrderBy(e => e.CreationDateTime)
                        .Take(MaxRequestsPerTime)
                        .ToListAsync());

            foreach (var request in requests)
            {
                request.IsInvitationSent = true;

                // Generate invitation.
                var invitation = new Invitation(DefaultInvitationDuration, null, false, true);
                await dbContext.Invitations.CreateAsync(invitation);

                // Send alpha pass.
                var link = "https://sso.etherna.io/Identity/Account/Register?invitationCode=" + invitation.Code;

                var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                    "Views/Emails/AlphaPassLetter.cshtml",
                    new AlphaPassLetterModel(
                        invitation.Code,
                        link),
                    actionContext);

                await emailSender.SendEmailAsync(
                    request.NormalizedEmail,
                    AlphaPassLetterModel.Title,
                    emailBody);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
