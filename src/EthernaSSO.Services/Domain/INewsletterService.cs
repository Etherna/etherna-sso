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

using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    /// <summary>
    /// Where a newsletter subscription originated. The newsletter service maps it to the contact tag,
    /// so callers don't deal with provider-specific tag strings.
    /// </summary>
    public enum NewsletterSubscriptionSource
    {
        Registration,
        AccountSettings
    }

    public interface INewsletterService
    {
        /// <summary>
        /// Add an email contact to the newsletter allowed list (managed by the external newsletter service).
        /// The email must already be verified by the caller: emails stored in the SSO db are never used to
        /// send non-technical communications, the contact is delegated to the newsletter service instead.
        /// The operation is idempotent (add-or-update): subscribing an already-subscribed contact is a no-op.
        /// </summary>
        /// <param name="email">The verified email address to subscribe.</param>
        /// <param name="source">Where the subscription originated (the service maps it to a contact tag).</param>
        Task SubscribeEmailAsync(string email, NewsletterSubscriptionSource source);

        /// <summary>
        /// Whether the given email is currently subscribed to the newsletter allowed list. Returns false
        /// when the service is disabled or the status cannot be determined (the subscribe call is idempotent).
        /// </summary>
        /// <param name="email">The email address to check.</param>
        Task<bool> IsSubscribedAsync(string email);
    }
}
