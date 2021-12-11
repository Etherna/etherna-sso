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
using Nethereum.Util;
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
        private readonly IServiceSharedDbContext sharedDbContext;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public UserService(
            IServiceSharedDbContext sharedDbContext,
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            this.sharedDbContext = sharedDbContext;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Methods.
        public Task<UserBase> FindUserByAddressAsync(string etherAddress)
        {
            etherAddress = etherAddress.ConvertToEthereumChecksumAddress();
            return ssoDbContext.Users.FindOneAsync(
                u => u.EtherAddress == etherAddress ||
                u.EtherPreviousAddresses.Contains(etherAddress));
        }

        public async Task<UserSharedInfo> GetSharedUserInfo(UserBase user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var sharedInfoId = user.UserSharedInfoId;
            return await sharedDbContext.UsersInfo.FindOneAsync(sharedInfoId);
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
                    var user = new UserWeb2(username, invitedByUser, invitedByAdmin);
                    var result = await userManager.CreateAsync(user, password);
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
                    var user = new UserWeb2(username, invitedByUser, invitedByAdmin, loginInfo);
                    var result = await userManager.CreateAsync(user);
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
                    var user = new UserWeb3(etherAddress, username, invitedByUser, invitedByAdmin);
                    var result = await userManager.CreateAsync(user);
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
            var sharedInfo = await GetSharedUserInfo(user);

            sharedInfo.LockoutEnabled = lockoutEnabled;
            sharedInfo.LockoutEnd = lockoutEnd;

            await sharedDbContext.SaveChangesAsync();
        }

        // Helpers.
        private async Task<(IEnumerable<(string key, string msg)> errors, TUser? user)> RegisterUserHelperAsync<TUser>(
            string username,
            string? invitationCode,
            Func<UserBase?, bool, Task<(TUser, IdentityResult)>> registerUserAsync)
            where TUser : UserBase
        {
            // Verify for unique username.
            if (await userManager.FindByNameAsync(username) is not null) //if duplicate username
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
            var (user, creationResult) = await registerUserAsync(invitation?.Emitter, invitation?.IsFromAdmin ?? false);

            // Delete used invitation if is single use, and if registration succeeded.
            if (creationResult.Succeeded &&
                invitation is not null &&
                invitation.IsSingleUse)
                await ssoDbContext.Invitations.DeleteAsync(invitation);

            return (creationResult.Errors.Select(e => (e.Code, e.Description)),
                creationResult.Succeeded ? user : null);
        }
    }
}
