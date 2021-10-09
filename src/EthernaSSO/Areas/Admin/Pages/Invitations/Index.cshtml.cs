using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.RCL.Views.Emails;
using Etherna.SSOServer.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
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

            [Display(Name = "New invitation email receivers")]
            public string? EmailReceivers { get; set; }
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

        [TempData]
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
            if (Input?.EmailReceivers is null)
            {
                await InitializeAsync();
                return Page();
            }

            // Clean input.
            var emails = Input.EmailReceivers
                .Split('\r', '\n')
                .Select(s => s.Trim())
                .Where(s => EmailHelper.IsValidEmail(s))
                .Distinct()
                .ToArray();

            // Generate invitations.
            var invitations = await GenerateInvitationsAsync(emails.Length);
            GeneratedInvitations = invitations;

            // Send emails.
            for (int i = 0; i < invitations.Length; i++)
            {
                var link = Url.PageLink(
                    pageName: "../Register",
                    values: new { invitationCode = invitations[i].Code });

                var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                    "Views/Emails/InvitationLetter.cshtml",
                    new InvitationLetterModel(link));

                await emailSender.SendEmailAsync(
                    emails[i],
                    InvitationLetterModel.Title,
                    emailBody);
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
                invitations[i] = new Invitation(DefaultInvitationDuration, user, true);

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
