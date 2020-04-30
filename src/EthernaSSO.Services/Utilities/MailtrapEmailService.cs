using Etherna.SSOServer.Services.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Utilities
{
    class MailtrapEmailService : IEmailService
    {
        private readonly EmailSettings settings;

        public MailtrapEmailService(IOptions<EmailSettings> opts)
        {
            settings = opts.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
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
    }
}
