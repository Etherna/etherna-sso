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
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Settings;
using Etherna.SSOServer.Services.Views.Emails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public sealed class ProcessAlphaPassRequestsTask : IProcessAlphaPassRequestsTask
    {
        // Consts.
        private readonly TimeSpan DefaultInvitationDuration = TimeSpan.FromDays(30);
        private const int MaxRequestsPerTime = 1;
        private readonly Uri SsoBaseUri = new("https://sso.etherna.io/");

        public const string TaskId = "processAlphaPassRequestsTask";

        // Fields.
        private readonly ApplicationSettings applicationSettings;
        private readonly ISsoDbContext dbContext;
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly IServiceProvider serviceProvider;

        // Constructor.
        public ProcessAlphaPassRequestsTask(
            IOptions<ApplicationSettings> applicationSettings,
            ISsoDbContext dbContext,
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(applicationSettings, nameof(applicationSettings));

            this.applicationSettings = applicationSettings.Value;
            this.dbContext = dbContext;
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
            this.serviceProvider = serviceProvider;
        }

        // Methods.
        public async Task RunAsync()
        {
            // Disable alpha pass emission if required.
            if (!applicationSettings.EnableAlphaPassEmission)
                return;

            // Create an action context. Required for view rendering.
            var httpContext = new DefaultHttpContext {
                RequestServices = serviceProvider,
                Request =
                {
                    Scheme = SsoBaseUri.Scheme,
                    Host = HostString.FromUriComponent(SsoBaseUri),
                    PathBase = PathString.FromUriComponent(SsoBaseUri),
                }
            };
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

                // Set as sent.
                request.IsInvitationSent = true;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
