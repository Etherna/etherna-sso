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

using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Domain;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.Identity
{
    /// <summary>
    /// A facade for <see cref="UserBase"/> used by Asp.Net Identity framework.
    /// </summary>
    public sealed class UserStore :
        IUserAuthenticatorKeyStore<UserBase>,
        IUserClaimStore<UserBase>,
        IUserEmailStore<UserBase>,
        IUserLockoutStore<UserBase>,
        IUserPasswordStore<UserBase>,
        IUserPhoneNumberStore<UserBase>,
        IUserRoleStore<UserBase>,
        IUserSecurityStampStore<UserBase>,
        IUserStore<UserBase>,
        IUserTwoFactorRecoveryCodeStore<UserBase>,
        IUserTwoFactorStore<UserBase>
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;
        private readonly IUserService userService;

        // Constructor.
        public UserStore(
            ISsoDbContext ssoDbContext,
            IUserService userService)
        {
            this.ssoDbContext = ssoDbContext;
            this.userService = userService;
        }

        // Methods.
        public Task AddClaimsAsync(UserBase user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            foreach (var claim in claims.Select(c => new Domain.Models.UserAgg.UserClaim(c.Type, c.Value)))
                user.AddClaim(claim);
            return Task.CompletedTask;
        }

        public async Task AddToRoleAsync(UserBase user, string roleName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

            var role = await ssoDbContext.Roles.QueryElementsAsync(elements =>
                elements.Where(r => r.NormalizedName == roleName)
                        .FirstAsync());

            user.AddRole(role);
        }

        public Task<int> CountCodesAsync(UserBase user, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            return Task.FromResult(userWeb2.TwoFactorRecoveryCodes.Count());
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External library doesn't declare exceptions")]
        public async Task<IdentityResult> CreateAsync(UserBase user, CancellationToken cancellationToken)
        {
            try { await ssoDbContext.Users.CreateAsync(user, cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External library doesn't declare exceptions")]
        public async Task<IdentityResult> DeleteAsync(UserBase user, CancellationToken cancellationToken)
        {
            try { await userService.DeleteAsync(user); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }

        public void Dispose() { }

        public async Task<UserBase?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
            await ssoDbContext.Users.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail));

        public Task<UserBase?> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            //using try for avoid exception throwing inside Identity's userManager
            ssoDbContext.Users.TryFindOneAsync(userId, cancellationToken: cancellationToken)!;

        public async Task<UserBase?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
            await ssoDbContext.Users.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUserName));

        public Task<int> GetAccessFailedCountAsync(UserBase user, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            return Task.FromResult(userWeb2.AccessFailedCount);
        }

        public Task<string?> GetAuthenticatorKeyAsync(UserBase user, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            return Task.FromResult(userWeb2.AuthenticatorKey);
        }

        public Task<IList<Claim>> GetClaimsAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult<IList<Claim>>(
                user.Claims.Select(c => new Claim(c.Type, c.Value)).ToList());
        }

        public Task<string?> GetEmailAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult(user.NormalizedEmail is not null);
        }

        public async Task<bool> GetLockoutEnabledAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            var sharedInfo = await userService.GetSharedUserInfoAsync(user);

            return sharedInfo.LockoutEnabled;
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            var sharedInfo = await userService.GetSharedUserInfoAsync(user);

            return sharedInfo.LockoutEnd;
        }

        public Task<string?> GetNormalizedEmailAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult(user.NormalizedEmail);
        }

        public Task<string?> GetNormalizedUserNameAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult<string?>(user.NormalizedUsername);
        }

        public Task<string?> GetPasswordHashAsync(UserBase user, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            return Task.FromResult(userWeb2.PasswordHash);
        }

        public Task<string?> GetPhoneNumberAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task<IList<string>> GetRolesAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult<IList<string>>(user.Roles.Select(r => r.NormalizedName).ToList());
        }

        public Task<string?> GetSecurityStampAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult<string?>(user.SecurityStamp);
        }

        public Task<bool> GetTwoFactorEnabledAsync(UserBase user, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            return Task.FromResult(userWeb2.TwoFactorEnabled);
        }

        public Task<string> GetUserIdAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult(user.Id);
        }

        public Task<string?> GetUserNameAsync(UserBase user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult<string?>(user.Username);
        }

        public async Task<IList<UserBase>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            return await ssoDbContext.Users.QueryElementsAsync(
                users => users.Where(u => u.Claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                              .ToListAsync());
        }

        public async Task<IList<UserBase>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            return await ssoDbContext.Users.QueryElementsAsync(
                users => users.Where(u => u.Roles.Any(r => r.NormalizedName == roleName))
                              .ToListAsync());
        }

        public Task<bool> HasPasswordAsync(UserBase user, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            return Task.FromResult(userWeb2.HasPassword);
        }

        public Task<int> IncrementAccessFailedCountAsync(UserBase user, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            userWeb2.IncrementAccessFailedCount();
            return Task.FromResult(userWeb2.AccessFailedCount);
        }

        public Task<bool> IsInRoleAsync(UserBase user, string roleName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            return Task.FromResult(user.Roles.Any(r => r.NormalizedName == roleName));
        }

        public Task<bool> RedeemCodeAsync(UserBase user, string code, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            return Task.FromResult(userWeb2.RedeemTwoFactorRecoveryCode(code));
        }

        public Task RemoveClaimsAsync(UserBase user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            foreach (var claim in claims.Select(c => new Domain.Models.UserAgg.UserClaim(c.Type, c.Value)))
                user.RemoveClaim(claim);
            return Task.CompletedTask;
        }

        public Task RemoveFromRoleAsync(UserBase user, string roleName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            user.RemoveRole(roleName);
            return Task.CompletedTask;
        }

        public Task ReplaceClaimAsync(UserBase user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            ArgumentNullException.ThrowIfNull(claim, nameof(claim));
            ArgumentNullException.ThrowIfNull(newClaim, nameof(newClaim));

            user.RemoveClaim(new Domain.Models.UserAgg.UserClaim(claim.Type, claim.Value));
            user.AddClaim(new Domain.Models.UserAgg.UserClaim(newClaim.Type, newClaim.Value));

            return Task.CompletedTask;
        }

        public Task ReplaceCodesAsync(UserBase user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            userWeb2.TwoFactorRecoveryCodes = recoveryCodes;
            return Task.CompletedTask;
        }

        public Task ResetAccessFailedCountAsync(UserBase user, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            userWeb2.ResetAccessFailedCount();
            return Task.CompletedTask;
        }

        public Task SetAuthenticatorKeyAsync(UserBase user, string key, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            userWeb2.AuthenticatorKey = key;
            return Task.CompletedTask;
        }

        public Task SetEmailAsync(UserBase user, string? email, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            if (email is null)
                user.RemoveEmail();
            else
                user.SetEmail(email);

            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(UserBase user, bool confirmed, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            //we accept only confirmed emails
            return Task.CompletedTask;
        }

        public async Task SetLockoutEnabledAsync(UserBase user, bool enabled, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            var sharedInfo = await userService.GetSharedUserInfoAsync(user);
            sharedInfo.LockoutEnabled = enabled;
        }

        public async Task SetLockoutEndDateAsync(UserBase user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            var sharedInfo = await userService.GetSharedUserInfoAsync(user);
            sharedInfo.LockoutEnd = lockoutEnd;
        }

        public Task SetNormalizedEmailAsync(UserBase user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            //don't perform any action, because email normalization is already performed by domain
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(UserBase user, string? normalizedName, CancellationToken cancellationToken)
        {
            //don't perform any action, because username normalization is already performed by domain
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(UserBase user, string? passwordHash, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            userWeb2.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(UserBase user, string? phoneNumber, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            user.SetPhoneNumber(phoneNumber);
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberConfirmedAsync(UserBase user, bool confirmed, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            //if confirmed == false don't perform any action, because is already managed by domain
            if (confirmed)
                user.ConfirmPhoneNumber();

            return Task.CompletedTask;
        }

        public Task SetSecurityStampAsync(UserBase user, string stamp, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task SetTwoFactorEnabledAsync(UserBase user, bool enabled, CancellationToken cancellationToken)
        {
            if (user is not UserWeb2 userWeb2)
                throw new ArgumentException("User is not a web2 account", nameof(user));

            userWeb2.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(UserBase user, string? userName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            ArgumentNullException.ThrowIfNull(userName, nameof(userName));

            user.SetUsername(userName);
            return Task.CompletedTask;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External library doesn't declare exceptions")]
        public async Task<IdentityResult> UpdateAsync(UserBase user, CancellationToken cancellationToken)
        {
            try
            {
                //update both UserBase and UserSharedInfo
                await ssoDbContext.SaveChangesAsync(cancellationToken);
            }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }
    }
}
