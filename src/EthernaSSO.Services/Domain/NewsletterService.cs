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
using Etherna.SSOServer.Services.Extensions;
using Etherna.SSOServer.Services.Options;
using MailChimp.Net;
using MailChimp.Net.Core;
using MailChimp.Net.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    public sealed class NewsletterService : INewsletterService
    {
        // Fields.
        private readonly ILogger<NewsletterService> logger;
        private readonly NewsletterOptions options;

        // Constructor.
        public NewsletterService(
            ILogger<NewsletterService> logger,
            IOptions<NewsletterOptions> opts)
        {
            ArgumentNullException.ThrowIfNull(opts);

            this.logger = logger;
            options = opts.Value;
        }

        // Methods.
        public Task SubscribeEmailAsync(string email, NewsletterSubscriptionSource source)
        {
            if (!EmailHelper.IsValidEmail(email))
                throw new ArgumentException("Email is not valid", nameof(email));

            return options.CurrentService switch
            {
                NewsletterOptions.NewsletterService.Mailchimp => MailchimpSubscribeAsync(email, source),
                NewsletterOptions.NewsletterService.FakeService => Task.CompletedTask,
                _ => throw new InvalidOperationException()
            };
        }

        public Task<bool> IsSubscribedAsync(string email)
        {
            if (!EmailHelper.IsValidEmail(email))
                throw new ArgumentException("Email is not valid", nameof(email));

            return options.CurrentService switch
            {
                NewsletterOptions.NewsletterService.Mailchimp => MailchimpIsSubscribedAsync(email),
                NewsletterOptions.NewsletterService.FakeService => Task.FromResult(false),
                _ => throw new InvalidOperationException()
            };
        }

        // Helpers.
        private static string SourceTag(NewsletterSubscriptionSource source) => source switch
        {
            NewsletterSubscriptionSource.Registration => "sso-registration",
            NewsletterSubscriptionSource.AccountSettings => "sso-account",
            _ => throw new InvalidOperationException()
        };

        private async Task MailchimpSubscribeAsync(string email, NewsletterSubscriptionSource source)
        {
            try
            {
                var manager = new MailChimpManager(options.ApiKey);

                // The email has already been verified by the SSO before this call, so a single opt-in
                // (no Mailchimp double opt-in) is enough: add or update the contact as subscribed.
                await manager.Members.AddOrUpdateAsync(
                    options.AudienceId,
                    new Member
                    {
                        EmailAddress = email,
                        Status = Status.Subscribed,
                        StatusIfNew = Status.Subscribed
                    });

                // Tag the contact with its origin, to segment the single audience by source.
                await manager.Members.AddTagsAsync(
                    options.AudienceId,
                    email,
                    new Tags { MemberTags = [new Tag { Name = SourceTag(source), Status = "active" }] });
            }
            catch (MailChimpException ex)
            {
                // A newsletter-service failure must never block account registration: we only log it
                // (no consent is stored on our side, so the user can simply opt in again later).
                logger.NewsletterSubscriptionFailed(email, ex);
            }
            catch (HttpRequestException ex)
            {
                logger.NewsletterSubscriptionFailed(email, ex);
            }
        }

        private async Task<bool> MailchimpIsSubscribedAsync(string email)
        {
            try
            {
                var manager = new MailChimpManager(options.ApiKey);

                // Defaults: falseIfUnsubscribed = true → returns true only if the contact is currently subscribed.
                return await manager.Members.ExistsAsync(options.AudienceId, email);
            }
            catch (MailChimpException ex)
            {
                // If we can't read the status, report "not subscribed": the subscribe call is idempotent,
                // so at worst the user is offered the button and an add-or-update is performed.
                logger.NewsletterStatusCheckFailed(email, ex);
                return false;
            }
            catch (HttpRequestException ex)
            {
                logger.NewsletterStatusCheckFailed(email, ex);
                return false;
            }
        }
    }
}
