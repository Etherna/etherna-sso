// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Services.Options;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    public sealed class EmailSender : IEmailSender
    {
        // Fields.
        private readonly EmailOptions options;

        // Constructor.
        public EmailSender(IOptions<EmailOptions> opts)
        {
            ArgumentNullException.ThrowIfNull(opts);

            options = opts.Value;
        }

        // Methods.
        public Task SendEmailAsync(string email, string subject, string message)
        {
            if (!EmailHelper.IsValidEmail(email))
                throw new ArgumentException("Email is not valid", nameof(email));

            return options.CurrentService switch
            {
                EmailOptions.EmailService.Mailtrap => MailtrapSendEmailAsync(email, subject, message),
                EmailOptions.EmailService.Sendgrid => SendgridSendEmailAsync(email, subject, message),
                EmailOptions.EmailService.FakeSender => Task.CompletedTask,
                _ => throw new InvalidOperationException()
            };
        }

        // Helpers.
        private async Task MailtrapSendEmailAsync(string email, string subject, string message)
        {
            using var client = new SmtpClient
            {
                Host = "smtp.mailtrap.io",
                Port = 2525,
                Credentials = new NetworkCredential(options.ServiceUser, options.ServiceKey),
                EnableSsl = true,
            };
            using var mail = new MailMessage(
                new MailAddress(options.SendingAddress, options.DisplayName),
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
            var client = new SendGridClient(options.ServiceKey);

            var from = new EmailAddress(options.SendingAddress, options.DisplayName);
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