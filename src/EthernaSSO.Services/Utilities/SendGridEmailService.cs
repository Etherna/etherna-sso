using Etherna.SSOServer.Services.Settings;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Utilities
{
    class SendGridEmailService : IEmailService
    {
        private readonly EmailSettings settings;

        public SendGridEmailService(IOptions<EmailSettings> opts)
        {
            settings = opts.Value;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string text)
        {
            var client = new SendGridClient(settings.ServiceKey);

            var mail = MailHelper.CreateSingleEmail(
                new EmailAddress(settings.SendingAddress, settings.DisplayName),
                new EmailAddress(recipientEmail),
                subject,
                text,
                text);

            await client.SendEmailAsync(mail);
        }
    }
}
