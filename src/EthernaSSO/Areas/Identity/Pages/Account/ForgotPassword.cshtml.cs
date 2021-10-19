using Etherna.SSOServer.Configs;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = default!;
        }

        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public ForgotPasswordModel(
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            UserManager<UserBase> userManager)
        {
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
            this.userManager = userManager;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            // Generate url.
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = CommonConsts.IdentityArea, code },
                protocol: Request.Scheme);

            // Send email.
            var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                "Views/Emails/ResetPassword.cshtml",
                new RCL.Views.Emails.ResetPasswordModel(callbackUrl));

            await emailSender.SendEmailAsync(
                Input.Email,
                RCL.Views.Emails.ResetPasswordModel.Title,
                emailBody);

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
