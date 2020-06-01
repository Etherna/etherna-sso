using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Identity
{
    public class CustomUserValidator : IUserValidator<User>
    {
        // Constructor.
        /// <summary>
        /// Creates a new instance of <see cref="UserValidator{User}"/>/
        /// </summary>
        /// <param name="errors">The <see cref="IdentityErrorDescriber"/> used to provider error messages.</param>
        public CustomUserValidator(IdentityErrorDescriber? errors = null)
        {
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
        public virtual async Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var errors = new List<IdentityError>();

            await ValidateUserName(manager, user, errors);
            await ValidateEmail(manager, user, errors);

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        // Helpers.
        private async Task ValidateUserName(UserManager<User> manager, User user, ICollection<IdentityError> errors)
        {
            var userName = await manager.GetUserNameAsync(user);

            //allow null usernames
            if (string.IsNullOrWhiteSpace(userName))
            {
                return;
            }

            //check validity with regex
            else if (!Regex.IsMatch(userName, User.UsernameRegex))
            {
                errors.Add(Describer.InvalidUserName(userName));
            }

            //check unique
            else
            {
                var owner = await manager.FindByNameAsync(userName);
                if (owner != null &&
                    !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user), StringComparison.InvariantCulture))
                {
                    errors.Add(Describer.DuplicateUserName(userName));
                }
            }
        }

        private async Task ValidateEmail(UserManager<User> manager, User user, List<IdentityError> errors)
        {
            var email = await manager.GetEmailAsync(user);

            if (email != null)
            {
                //check validity
                if (!new EmailAddressAttribute().IsValid(email))
                {
                    errors.Add(Describer.InvalidEmail(email));
                    return;
                }
            }

            if (manager.Options.User.RequireUniqueEmail)
            {
                //check not empty
                if (string.IsNullOrWhiteSpace(email))
                {
                    errors.Add(Describer.InvalidEmail(email));
                    return;
                }

                //check unique
                var owner = await manager.FindByEmailAsync(email);
                if (owner != null &&
                    !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user), StringComparison.InvariantCulture))
                {
                    errors.Add(Describer.DuplicateEmail(email));
                }
            }

        }
    }
}
