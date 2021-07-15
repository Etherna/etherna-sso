using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    public class UserService : IUserService
    {
        // Consts.
        public const string DuplicateEmailErrorKey = "DuplicateEmail";
        public const string DuplicateUsernameErrorKey = "DuplicateUsername";
        public const string InvalidValidationErrorKey = "InvalidInvitation";

        // Fields.
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public UserService(
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Methods.
        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            string password,
            string? email,
            string? invitationCode) =>
            RegisterUserHelperAsync(
                username,
                email,
                invitationCode,
                async invitedByUser =>
                {
                    var user = new UserWeb2(username, email, invitedByUser);
                    var result = await userManager.CreateAsync(user, password);
                    return (user, result);
                });

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            SSOServer.Domain.Models.UserAgg.UserLoginInfo loginInfo,
            string? email,
            string? invitationCode) =>
            RegisterUserHelperAsync(
                username,
                email,
                invitationCode,
                async invitedByUser =>
                {
                    var user = new UserWeb2(username, email, invitedByUser, loginInfo);
                    var result = await userManager.CreateAsync(user);
                    return (user, result);
                });

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb3? user)> RegisterWeb3UserAsync(
            string username,
            string etherAddress,
            string? email,
            string? invitationCode) =>
            RegisterUserHelperAsync(
                username,
                email,
                invitationCode,
                async invitedByUser =>
                {
                    var user = new UserWeb3(etherAddress, username, email, invitedByUser);
                    var result = await userManager.CreateAsync(user);
                    return (user, result);
                });

        // Helpers.
        private async Task<IEnumerable<(string key, string msg)>> CheckDuplicateUsernameOrEmailAsync(
            string username,
            string? email)
        {
            var errors = new List<(string key, string msg)>();

            // Check for duplicate username.
            if (await userManager.FindByNameAsync(username) is not null) //if duplicate username
                errors.Add((DuplicateUsernameErrorKey, "Username already registered."));

            // Check for duplicate email.
            if (email != null)
            {
                var userByEmail = await userManager.FindByEmailAsync(email);
                if (userByEmail != null) //if duplicate email
                    errors.Add((DuplicateEmailErrorKey, "Email already registered."));
            }

            return errors;
        }

        private async Task<(IEnumerable<(string key, string msg)> errors, UserBase? invitedByUser)> ConsumeInvitationCodeAsync(
            string? invitationCode)
        {
            var errors = new List<(string key, string msg)>();

            // Verify invitation code.
            if (invitationCode is null)
                return (errors, null);

            var invitation = await ssoDbContext.Invitations.TryFindOneAsync(i => i.Code == invitationCode);
            if (invitation is null || !invitation.IsAlive)
            {
                errors.Add((InvalidValidationErrorKey, "Invitation is not valid."));
                return (errors, null);
            }

            // Delete used invitation.
            if (invitation.IsSingleUse)
                await ssoDbContext.Invitations.DeleteAsync(invitation);

            return (errors, invitation.Emitter);
        }

        private async Task<(IEnumerable<(string key, string msg)> errors, TUser? user)> RegisterUserHelperAsync<TUser>(
            string username,
            string? email,
            string? invitationCode,
            Func<UserBase?, Task<(TUser, IdentityResult)>> registerUserAsync)
            where TUser : UserBase
        {
            // Verify for unique username and email.
            var duplicateErrors = await CheckDuplicateUsernameOrEmailAsync(username, email);
            if (duplicateErrors.Any())
                return (duplicateErrors, null);

            // Consume invitation code.
            var (invitationErrors, invitedByUser) = await ConsumeInvitationCodeAsync(invitationCode);
            if (invitationErrors.Any())
                return (invitationErrors, null);

            // Register new user.
            var (user, creationResult) = await registerUserAsync(invitedByUser);

            return (creationResult.Errors.Select(e => (e.Code, e.Description)),
                creationResult.Succeeded ? user : null);
        }
    }
}
