using Etherna.SSOServer.Services.Settings;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Utilities
{
    class EmailService : IEmailService
    {
        private readonly EmailSettings settings;

        public EmailService(IOptions<EmailSettings> opts)
        {
            settings = opts.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message) =>
            settings.CurrentService switch
            {
                EmailSettings.EmailService.Mailtrap => MailtrapSendEmailAsync(email, subject, message),
                EmailSettings.EmailService.Sendgrid => SendgridSendEmailAsync(email, subject, message),
                _ => Task.CompletedTask
            };

        // Helpers.
        private async Task MailtrapSendEmailAsync(string email, string subject, string message)
        {
            using var client = new SmtpClient
            {
                Host = "mailtrap.io",
                Port = 2525,
                Credentials = new NetworkCredential(settings.ServiceUser, settings.ServiceKey),
                EnableSsl = true,
            };
            using var mail = new MailMessage(
                new MailAddress(settings.SendingAddress, settings.DisplayName),
                new MailAddress(email))
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };

            await client.SendMailAsync(mail);
        }

        private async Task SendgridSendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(settings.ServiceKey);

            var mail = MailHelper.CreateSingleEmail(
                new EmailAddress(settings.SendingAddress, settings.DisplayName),
                new EmailAddress(email),
                subject,
                message,
                message);

            await client.SendEmailAsync(mail);
        }
    }
}
