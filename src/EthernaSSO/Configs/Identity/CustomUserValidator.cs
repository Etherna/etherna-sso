//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.Identity
{
    public class CustomUserValidator : IUserValidator<UserBase>
    {
        // Fields.
        private readonly ApplicationSettings applicationSettings;

        // Constructor.
        /// <summary>
        /// Creates a new instance of <see cref="UserValidator{User}"/>/
        /// </summary>
        /// <param name="applicationSettings">Application settings</param>
        /// <param name="errors">The <see cref="IdentityErrorDescriber"/>Used to provider error messages</param>
        public CustomUserValidator(
            IOptions<ApplicationSettings> applicationSettings,
            IdentityErrorDescriber? errors = null)
        {
            this.applicationSettings = applicationSettings?.Value ?? throw new ArgumentNullException(nameof(applicationSettings));
            Describer = errors ?? new IdentityErrorDescriber();
        }

        // Properties.
        /// <summary>
        /// Gets the <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator{User}"/>.
        /// </summary>
        /// <value>The <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator{User}"/>.</value>
        public IdentityErrorDescriber Describer { get; private set; }

        // Methods.
        /// <summary>
        /// Validates the specified <paramref name="user"/> as an asynchronous operation.
        /// </summary>
        /// <param name="manager">The <see cref="UserManager{User}"/> that can be used to retrieve user properties.</param>
        /// <param name="user">The user to validate.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the validation operation.</returns>
        public virtual async Task<IdentityResult> ValidateAsync(UserManager<UserBase> manager, UserBase user)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var errors = new List<IdentityError>();

            // Validate fields.
            await ValidateUsernameAsync(manager, user, errors);
            await ValidateEmailAsync(manager, user, errors);

            // Validate logins.
            if (user is UserWeb2 userWeb2)
                ValidateWeb2Logins(userWeb2, errors);

            // Validate invitation.
            if (applicationSettings.RequireInvitation && user.InvitedBy is null)
                errors.Add(new IdentityError { Code = "RequiredInvitation" });

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        // Helpers.
        private async Task ValidateEmailAsync(UserManager<UserBase> manager, UserBase user, List<IdentityError> errors)
        {
            var email = await manager.GetEmailAsync(user);

            //allow null emails
            if (string.IsNullOrWhiteSpace(email))
                return;

            //check validity
            if (!new EmailAddressAttribute().IsValid(email))
            {
                errors.Add(Describer.InvalidEmail(email));
                return;
            }

            //check unique
            if (manager.Options.User.RequireUniqueEmail)
            {
                var owner = await manager.FindByEmailAsync(email);
                if (owner != null &&
                    !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user), StringComparison.Ordinal))
                {
                    errors.Add(Describer.DuplicateEmail(email));
                }
            }
        }

        private async Task ValidateUsernameAsync(UserManager<UserBase> manager, UserBase user, ICollection<IdentityError> errors)
        {
            var username = await manager.GetUserNameAsync(user);

            //check validity with regex
            if (!Regex.IsMatch(username, UserBase.UsernameRegex))
            {
                errors.Add(Describer.InvalidUserName(username));
                return;
            }

            //check unique
            var owner = await manager.FindByNameAsync(username);
            if (owner != null &&
                !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user), StringComparison.Ordinal))
            {
                errors.Add(Describer.DuplicateUserName(username));
            }
        }

        private void ValidateWeb2Logins(UserWeb2 user, List<IdentityError> errors)
        {
            var hasValidLogin =
                user.CanLoginWithEmail ||
                user.CanLoginWithEtherAddress ||
                user.CanLoginWithExternalProvider ||
                user.CanLoginWithUsername;

            if (!hasValidLogin)
                errors.Add(Describer.DefaultError());
        }
    }
}
