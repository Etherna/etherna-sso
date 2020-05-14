using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Domain.IdentityStores
{
    /// <summary>
    /// A facade for <see cref="User"/> used by Asp.Net Identity framework.
    /// </summary>
    public sealed class UserStore :
        IUserAuthenticatorKeyStore<User>,
        IUserEmailStore<User>,
        IUserLockoutStore<User>,
        IUserLoginStore<User>,
        IUserPasswordStore<User>,
        IUserPhoneNumberStore<User>,
        IUserSecurityStampStore<User>,
        IUserStore<User>,
        IUserTwoFactorRecoveryCodeStore<User>,
        IUserTwoFactorStore<User>
    {
        private readonly ISsoDbContext ssoDbContext;

        public UserStore(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        public Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (login is null)
                throw new ArgumentNullException(nameof(login));

            user.AddLogin(new Domain.Models.UserAgg.UserLoginInfo(login.LoginProvider, login.ProviderKey));
            return Task.CompletedTask;
        }

        public Task<int> CountCodesAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.TwoFactorRecoveryCodes.Count());
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External library doesn't declare exceptions")]
        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            try { await ssoDbContext.Users.CreateAsync(user, cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External library doesn't declare exceptions")]
        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            try { await ssoDbContext.Users.DeleteAsync(user, cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }

        public void Dispose() { }

        public Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
            ssoDbContext.Users.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail));

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            //using try for avoid exception throwing inside Identity's userManager
            ssoDbContext.Users.TryFindOneAsync(userId, cancellationToken: cancellationToken)!;

        public Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) =>
            ssoDbContext.Users.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.Logins.Any(
                    l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)));

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
            ssoDbContext.Users.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUserName));

        public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<string?> GetAuthenticatorKeyAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.AuthenticatorKey);
        }

        public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.LockoutEnd);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult<IList<UserLoginInfo>>(
                user.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.LoginProvider))
                           .ToList());
        }

        public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.NormalizedEmail);
        }

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.NormalizedUsername);
        }

        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PasswordHash);
        }

        public Task<string?> GetPhoneNumberAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.SecurityStamp);
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.Id);
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.Username);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.HasPassword);
        }

        public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(++user.AccessFailedCount);
        }

        public Task<bool> RedeemCodeAsync(User user, string code, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.RedeemTwoFactorRecoveryCode(code));
        }

        public Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.RemoveLogin(loginProvider, providerKey);
            return Task.CompletedTask;
        }

        public Task ReplaceCodesAsync(User user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.TwoFactorRecoveryCodes = recoveryCodes;
            return Task.CompletedTask;
        }

        public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task SetAuthenticatorKeyAsync(User user, string key, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.AuthenticatorKey = key;
            return Task.CompletedTask;
        }

        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.SetEmail(email);
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            //if confirmed == false don't perform any action, because is already managed by domain
            if (confirmed)
                user.ConfirmEmail();

            return Task.CompletedTask;
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.LockoutEnd = lockoutEnd.HasValue ?
                (DateTimeOffset?)new DateTime(lockoutEnd.Value.Ticks, DateTimeKind.Utc) : null;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            //don't perform any action, because email normalization is already performed by domain
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            //don't perform any action, because username normalization is already performed by domain
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(User user, string phoneNumber, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.SetPhoneNumber(phoneNumber);
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            //if confirmed == false don't perform any action, because is already managed by domain
            if (confirmed)
                user.ConfirmPhoneNumber();

            return Task.CompletedTask;
        }

        public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task SetTwoFactorEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            user.SetUsername(userName);
            return Task.CompletedTask;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External library doesn't declare exceptions")]
        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            try { await ssoDbContext.Users.ReplaceAsync(user, cancellationToken: cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }
    }
}
