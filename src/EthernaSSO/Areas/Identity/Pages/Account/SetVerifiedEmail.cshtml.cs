using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.RCL.Views.Emails;
using Etherna.SSOServer.Services.Domain;
using Etherna.SSOServer.Services.Utilities;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    public class SetVerifiedEmailModel : SsoExitPageModelBase
    {
        // Models.
        public class CodeInputModel
        {
            [Required]
            public string Code { get; set; } = default!;
        }
        public class EmailInputModel
        {
            [EmailAddress]
            [Display(Name = "Email (optional)")]
            [Required]
            public string Email { get; set; } = default!;
        }

        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IIdentityServerInteractionService idServerInteractionService;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly UserManager<UserBase> userManager;

        // Constructors.
        public SetVerifiedEmailModel(
            IEmailSender emailSender,
            IClientStore clientStore,
            IIdentityServerInteractionService idServerInteractionService,
            IRazorViewRenderer razorViewRenderer,
            UserManager<UserBase> userManager)
            : base(clientStore)
        {
            this.emailSender = emailSender;
            this.idServerInteractionService = idServerInteractionService;
            this.razorViewRenderer = razorViewRenderer;
            this.userManager = userManager;
        }

        // Properties.
        [BindProperty]
        public CodeInputModel CodeInput { get; set; } = default!;

        [BindProperty]
        public EmailInputModel EmailInput { get; set; } = default!;

        public bool IsCodeSent { get; private set; }
        public bool IsWeb3 { get; private set; }
        public string? ReturnUrl { get; private set; }

        // Methods.
        public async Task OnGetAsync(string? email, bool isCodeSent, string? returnUrl)
        {
            if (email is not null)
                EmailInput = new EmailInputModel { Email = email };

            var user = await userManager.GetUserAsync(User);
            IsWeb3 = user is UserWeb3;

            IsCodeSent = isCodeSent;
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnGetSkipAsync(string? returnUrl) =>
            await ProceedAsync(returnUrl);

        public async Task<IActionResult> OnPostSendEmailAsync(string? returnUrl)
        {
            // Validate model.
            if (!EmailHelper.IsValidEmail(EmailInput.Email))
                RedirectToPage(new
                {
                    EmailInput.Email,
                    returnUrl
                });

            // Check for duplicate email.
            if (await userManager.FindByEmailAsync(EmailInput.Email) is not null)
                ModelState.AddModelError(UserService.DuplicateEmailErrorKey, "Email already registered.");

            // Generate Totp code.
            var user = await userManager.GetUserAsync(User);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send email.
            var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                "Views/Emails/TotpConfirmEmail.cshtml",
                new TotpConfirmEmailModel(code));

            await emailSender.SendEmailAsync(
                EmailInput.Email,
                TotpConfirmEmailModel.Title,
                emailBody);

            // Reload page.
            return RedirectToPage(new
            {
                EmailInput.Email,
                returnUrl,
                isCodeSent = true
            });
        }

        public async Task<IActionResult> OnPostConfirmCodeAsync(string email, string? returnUrl)
        {
            // Validate model.
            if (string.IsNullOrEmpty(CodeInput.Code))
                RedirectToPage(new
                {
                    EmailInput.Email,
                    ReturnUrl
                });

            // Validate code.

            // Add email.
            var user = await userManager.GetUserAsync(User);
            user.SetEmail(email);

            return await ProceedAsync(returnUrl);
        }

        // Helpers.
        private async Task<IActionResult> ProceedAsync(string? returnUrl)
        {
            var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);
            return await ContextedRedirectAsync(context, returnUrl);
        }
    }
}
