using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EntityStores
{
    /// <summary>
    /// A facade for <see cref="User"/> used by Asp.Net Identity framework.
    /// </summary>
    public class UserStore :
        IUserEmailStore<User>,
        IUserLockoutStore<User>,
        IUserLoginStore<User>,
        IUserPasswordStore<User>,
        IUserSecurityStampStore<User>,
        IUserStore<User>
    {
        private readonly ISSOContext context;

        public UserStore(
            ISSOContext context)
        {
            this.context = context;
        }

        public Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            user.AddLogin(new Domain.Models.UserAgg.UserLoginInfo(login.LoginProvider, login.ProviderKey));
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            try { await context.Users.CreateAsync(user, cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            try { await context.Users.DeleteAsync(user, cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }

        public void Dispose() { }

        public Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
            context.Users.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail));

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            context.Users.FindOneAsync(userId, cancellationToken: cancellationToken);

        public Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) =>
            context.Users.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.Logins.Any(
                    l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)));

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
            context.Users.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUserName));

        public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.AccessFailedCount);

        public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Email);

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.EmailConfirmed);

        public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.LockoutEnabled);

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.LockoutEnd);

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult<IList<UserLoginInfo>>(
                user.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.LoginProvider))
                           .ToList());

        public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.NormalizedEmail);

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.NormalizedUsername);

        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PasswordHash);

        public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.SecurityStamp);

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Id);

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Username);

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.HasPassword);

        public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(++user.AccessFailedCount);

        public Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            user.RemoveLogin(loginProvider, providerKey);
            return Task.CompletedTask;
        }

        public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            user.SetEmail(email);
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            if (!confirmed)
                throw new InvalidOperationException("Can't unconfirm an email");

            user.ConfirmEmail();
            return Task.CompletedTask;
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
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
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.SetUsername(userName);
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            try { await context.Users.ReplaceAsync(user, cancellationToken: cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }
    }
}
