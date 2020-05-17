using Etherna.SSOServer.Services.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Utilities
{
    class EmailSender : IEmailSender
    {
        private readonly EmailSettings settings;

        public EmailSender(IOptions<EmailSettings> opts)
        {
            settings = opts.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message) =>
            settings.CurrentService switch
            {
                EmailSettings.EmailService.Mailtrap => MailtrapSendEmailAsync(email, subject, message),
                EmailSettings.EmailService.Sendgrid => SendgridSendEmailAsync(email, subject, message),
                EmailSettings.EmailService.FakeSender => Task.CompletedTask,
                _ => throw new InvalidOperationException()
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

        private async Task<Response> SendgridSendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(settings.ServiceKey);

            var from = new EmailAddress(settings.SendingAddress, settings.DisplayName);
            var to = new EmailAddress(email);

            var plainTextContent = message;
            var htmlContent = message;

            var mail = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent,
                htmlContent);

            return await client.SendEmailAsync(mail);
        }
    }
}
