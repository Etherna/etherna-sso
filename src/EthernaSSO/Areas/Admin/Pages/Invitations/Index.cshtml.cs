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

using Etherna.ACR.Helpers;
using Etherna.ACR.Services;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Configs;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Views.Emails;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.Invitations
{
    public class IndexModel : PageModel
    {
        // Consts.
        private readonly TimeSpan DefaultInvitationDuration = TimeSpan.FromDays(30);

        // Model.
        public class InputModel
        {
            [Display(Name = "New invitations quantity")]
            public int Quantity { get; set; }

            [Display(Name = "New invitation email and name receivers. Format <email>[;<name>]")]
            public string? EmailAndNameReceivers { get; set; }
        }

        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public IndexModel(
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Properties.
        [Display(Name = "New generated invitations")]
        public IEnumerable<Invitation>? GeneratedInvitations { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        [Display(Name = "Total alive invites")]
        public int TotalAlive { get; set; }

        public string? StatusMessage { get; set; }

        // Methods.
        public Task OnGetAsync() =>
            InitializeAsync();

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            // Validate input.
            if (Input is null)
            {
                await InitializeAsync();
                return Page();
            }

            // Generate invitations.
            GeneratedInvitations = await GenerateInvitationsAsync(Input.Quantity);

            StatusMessage = $"{Input.Quantity} invitations generated";
            await InitializeAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostGenerateAndSendAsync()
        {
            // Validate input.
            if (Input?.EmailAndNameReceivers is null)
            {
                await InitializeAsync();
                return Page();
            }

            // Clean input.
            var emailsAndNames = Input.EmailAndNameReceivers
                .Split('\r', '\n')
                .Select(line =>
                {
                    var split = line.Split(';');
                    var email = split[0].Trim();
                    var name = split.Length >= 2 && !string.IsNullOrEmpty(split[1]) ?
                        split[1].Trim() :
                        email.Split('@')[0];

                    return new { Email = email, Name = name };
                })
                .Where(r => EmailHelper.IsValidEmail(r.Email))
                .GroupBy(r => r.Email) //distinct by email
                .Select(r => r.First())
                .ToArray();

            // Generate invitations.
            var invitations = await GenerateInvitationsAsync(emailsAndNames.Length);
            GeneratedInvitations = invitations;

            // Send emails.
            for (int i = 0; i < invitations.Length; i++)
            {
                var link = Url.PageLink(
                    pageName: "/Account/Register",
                    values: new
                    {
                        area = CommonConsts.IdentityArea,
                        invitationCode = invitations[i].Code
                    });

                if (link is null)
                    throw new InvalidOperationException();

                var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                    "Views/Emails/AlphaPassLetter.cshtml",
                    new AlphaPassLetterModel(
                        invitations[i].Code,
                        link,
                        emailsAndNames[i].Name));

                await emailSender.SendEmailAsync(
                    emailsAndNames[i].Email,
                    AlphaPassLetterModel.Title,
                    emailBody);

                //var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                //    "Views/Emails/InvitationLetter.cshtml",
                //    new InvitationLetterModel(
                //        invitations[i].Code,
                //        link,
                //        emailsAndNames[i].Name));

                //await emailSender.SendEmailAsync(
                //    emailsAndNames[i].Email,
                //    InvitationLetterModel.Title,
                //    emailBody);
            }

            StatusMessage = $"{invitations.Length} invitations generated and sent";
            await InitializeAsync();
            return Page();
        }

        // Helpers.
        private async Task<Invitation[]> GenerateInvitationsAsync(int quantity)
        {
            var user = await userManager.GetUserAsync(User);
            var invitations = new Invitation[quantity];

            for (int i = 0; i < quantity; i++)
                invitations[i] = new Invitation(DefaultInvitationDuration, user, true, true);

            await ssoDbContext.Invitations.CreateAsync(invitations);

            return invitations;
        }

        private async Task InitializeAsync()
        {
            TotalAlive = await ssoDbContext.Invitations.QueryElementsAsync(invitations =>
                invitations.Where(i => i.EndLife == null || i.EndLife > System.DateTime.Now)
                           .CountAsync());
        }
    }
}
