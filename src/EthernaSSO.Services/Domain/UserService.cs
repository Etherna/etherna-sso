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

using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Exceptions;
using Etherna.MongODM.Core.Repositories;
using Etherna.SSL.Helpers;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly IServiceProvider serviceProvider;
        private readonly ISharedDbContext sharedDbContext;
        private readonly ISsoDbContext ssoDbContext;
        private UserManager<UserBase>? _userManager;

        // Constructor.
        public UserService(
            IServiceProvider serviceProvider,
            ISharedDbContext sharedDbContext,
            ISsoDbContext ssoDbContext)
        {
            this.serviceProvider = serviceProvider;
            this.sharedDbContext = sharedDbContext;
            this.ssoDbContext = ssoDbContext;
        }

        // Private properties.
        private UserManager<UserBase> UserManager
        {
            get
            {
                if (_userManager is null)
                    _userManager = serviceProvider.GetRequiredService<UserManager<UserBase>>();
                return _userManager;
            }
        }

        // Methods.
        public async Task DeleteAsync(UserBase user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            await ssoDbContext.Users.DeleteAsync(user);
            await sharedDbContext.UsersInfo.DeleteAsync(user.SharedInfoId);
        }

        public Task<UserBase> FindUserByAddressAsync(string etherAddress)
        {
            etherAddress = etherAddress.ConvertToEthereumChecksumAddress();
            return ssoDbContext.Users.FindOneAsync(
                u => u.EtherAddress == etherAddress ||
                u.EtherPreviousAddresses.Contains(etherAddress));
        }

        public async Task<UserSharedInfo> GetSharedUserInfoAsync(UserBase user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return await sharedDbContext.UsersInfo.FindOneAsync(user.SharedInfoId);
        }

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            string password,
            string? invitationCode) =>
            RegisterUserHelperAsync(
                username,
                invitationCode,
                async (invitedByUser, invitedByAdmin) =>
                {
                    // Generate managed key.
                    var etherPrivateKey = GenerateEtherPrivateKey();
                    var etherAddress = new Account(etherPrivateKey).Address;

                    // Create shared info.
                    var sharedInfo = new UserSharedInfo(etherAddress);
                    await sharedDbContext.UsersInfo.CreateAsync(sharedInfo);

                    // Create user.
                    var user = new UserWeb2(username, invitedByUser, invitedByAdmin, etherPrivateKey, sharedInfo);
                    var result = await UserManager.CreateAsync(user, password);

                    return (user, result);
                });

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            SSOServer.Domain.Models.UserAgg.UserLoginInfo loginInfo,
            string? invitationCode) =>
            RegisterUserHelperAsync(
                username,
                invitationCode,
                async (invitedByUser, invitedByAdmin) =>
                {
                    // Generate managed key.
                    var etherPrivateKey = GenerateEtherPrivateKey();
                    var etherAddress = new Account(etherPrivateKey).Address;

                    // Create shared info.
                    var sharedInfo = new UserSharedInfo(etherAddress);
                    await sharedDbContext.UsersInfo.CreateAsync(sharedInfo);

                    // Create user.
                    var user = new UserWeb2(username, invitedByUser, invitedByAdmin, etherPrivateKey, sharedInfo, loginInfo);
                    var result = await UserManager.CreateAsync(user);
                    return (user, result);
                });

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserByAdminAsync(
            string username,
            string password,
            string? email,
            string? etherLoginAddress,
            bool lockoutEnabled,
            DateTimeOffset? lockoutEnd,
            string? phoneNumber,
            IEnumerable<Role> roles,
            bool twoFactorEnabled) =>
            RegisterUserHelperAsync(
                username,
                null,
                async (_, _) =>
                {
                    // Generate managed key.
                    var etherPrivateKey = GenerateEtherPrivateKey();
                    var etherAddress = new Account(etherPrivateKey).Address;

                    // Create shared info.
                    var sharedInfo = new UserSharedInfo(etherAddress)
                    {
                        LockoutEnabled = lockoutEnabled,
                        LockoutEnd = lockoutEnd
                    };
                    await sharedDbContext.UsersInfo.CreateAsync(sharedInfo);

                    // Create user.
                    var user = new UserWeb2(username, null, true, etherPrivateKey, sharedInfo);

                    if (email is not null)
                        user.SetEmail(email);
                    user.SetPhoneNumber(phoneNumber);
                    if (!string.IsNullOrWhiteSpace(etherLoginAddress))
                        user.SetEtherLoginAddress(etherLoginAddress);
                    foreach (var role in roles)
                        user.AddRole(role);
                    user.TwoFactorEnabled = twoFactorEnabled;

                    var result = await UserManager.CreateAsync(user, password);
                    return (user, result);
                });

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb3? user)> RegisterWeb3UserAsync(
            string username,
            string etherAddress,
            string? invitationCode) =>
            RegisterUserHelperAsync(
                username,
                invitationCode,
                async (invitedByUser, invitedByAdmin) =>
                {
                    // Create shared info.
                    var sharedInfo = new UserSharedInfo(etherAddress);
                    await sharedDbContext.UsersInfo.CreateAsync(sharedInfo);

                    // Create user.
                    var user = new UserWeb3(username, invitedByUser, invitedByAdmin, sharedInfo);
                    var result = await UserManager.CreateAsync(user);
                    return (user, result);
                });

        public async Task<PaginatedEnumerable<UserBase>> SearchPaginatedUsersByQueryAsync<TOrderKey>(
            string? query,
            Expression<Func<UserBase, TOrderKey>> orderKeySelector,
            int page,
            int take,
            Expression<Func<UserBase, bool>>? filterPredicate = null)
        {
            filterPredicate ??= _ => true;
            query ??= "";

            //try search by address
            if (query.IsValidEthereumAddressHexFormat())
            {
                //get user
                UserBase? user = null;
                try { user = await FindUserByAddressAsync(query); }
                catch (MongodmEntityNotFoundException) { }

                //verify filter predicate
                if (user is not null)
                {
                    var filter = filterPredicate.Compile();
                    if (!filter(user)) //if filter doesn't allow user
                        user = null;
                }

                return new PaginatedEnumerable<UserBase>(
                    user is not null ? new[] { user } : Array.Empty<UserBase>(), 0, take, 0 );
            }

            //try search by email
            else if(EmailHelper.IsValidEmail(query))
            {
                var normalizedEmail = EmailHelper.NormalizeEmail(query);

                return await ssoDbContext.Users.QueryPaginatedElementsAsync(elements =>
                    elements.Where(filterPredicate)
                            .Where(u => u.NormalizedEmail == normalizedEmail),
                    orderKeySelector,
                    page,
                    take);
            }

            //try search by Id
            else if (ObjectId.TryParse(query, out var parsedObjectId))
            {
                var objectId = parsedObjectId.ToString();

                return await ssoDbContext.Users.QueryPaginatedElementsAsync(elements =>
                    elements.Where(filterPredicate)
                            .Where(u => u.Id == objectId),
                    orderKeySelector,
                    page,
                    take);
            }

            //try search by username
            else
            {
                var normalizedUsername = UsernameHelper.NormalizeUsername(query);

                return await ssoDbContext.Users.QueryPaginatedElementsAsync(elements =>
                    elements.Where(filterPredicate)
                            .Where(u => u.NormalizedUsername.Contains(normalizedUsername)),
                    orderKeySelector,
                    page,
                    take);
            }
        }

        public async Task UpdateLockoutStatusAsync(UserBase user, bool lockoutEnabled, DateTimeOffset? lockoutEnd)
        {
            var sharedInfo = await GetSharedUserInfoAsync(user);

            sharedInfo.LockoutEnabled = lockoutEnabled;
            sharedInfo.LockoutEnd = lockoutEnd;

            await sharedDbContext.SaveChangesAsync();
        }

        public async Task<UserWeb3> UpgradeToWeb3(UserWeb2 userWeb2)
        {
            // Update user.
            var userWeb3 = new UserWeb3(userWeb2);

            //deleting and recreating because of this https://etherna.atlassian.net/browse/MODM-83
            //await ssoDbContext.Users.ReplaceAsync(userWeb3);
            await ssoDbContext.Users.DeleteAsync(userWeb2);
            await ssoDbContext.Users.CreateAsync(userWeb3);

            // Update shared info.
            var sharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(userWeb3.SharedInfoId);
            sharedInfo.EtherAddress = userWeb3.EtherAddress;
            sharedInfo.EtherPreviousAddresses = userWeb3.EtherPreviousAddresses;
            await sharedDbContext.SaveChangesAsync();

            return userWeb3;
        }

        // Helpers.
        private string GenerateEtherPrivateKey()
        {
            var ecKey = EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            return privateKey;
        }

        private async Task<(IEnumerable<(string key, string msg)> errors, TUser? user)> RegisterUserHelperAsync<TUser>(
            string username,
            string? invitationCode,
            Func<UserBase?, bool, Task<(TUser, IdentityResult)>> registerUserAsync)
            where TUser : UserBase
        {
            // Verify for unique username.
            if (await UserManager.FindByNameAsync(username) is not null) //if duplicate username
                return (new[] { (DuplicateUsernameErrorKey, "Username already registered.") }, null);

            // Verify invitation code.
            Invitation? invitation = null;
            if (invitationCode is not null)
            {
                invitation = await ssoDbContext.Invitations.TryFindOneAsync(i => i.Code == invitationCode);
                if (invitation is null || !invitation.IsAlive)
                    return (new[] { (InvalidValidationErrorKey, "Invitation is not valid.") }, null);
            }

            // Register new user.
            var (user, creationResult) = await registerUserAsync(
                invitation?.Emitter,
                invitation?.IsFromAdmin ?? false);

            //if registration succeeded
            if (creationResult.Succeeded)
            {
                //if used invitation is single use, delete it
                if (invitation is not null &&
                    invitation.IsSingleUse)
                    await ssoDbContext.Invitations.DeleteAsync(invitation);
            }

            return (creationResult.Errors.Select(e => (e.Code, e.Description)),
                creationResult.Succeeded ? user : null);
        }
    }
}
