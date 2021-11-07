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

using Etherna.MongODM.Core.Repositories;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
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
        public Task<UserBase> FindUserByAddressAsync(string etherAddress)
        {
            etherAddress = etherAddress.ConvertToEthereumChecksumAddress();
            return ssoDbContext.Users.FindOneAsync(
                u => u.EtherAddress == etherAddress ||
                u.EtherPreviousAddresses.Contains(etherAddress));
        }

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            string password,
            string? invitationCode) =>
            RegisterUserHelperAsync(
                username,
                invitationCode,
                async invitedByUser =>
                {
                    var user = new UserWeb2(username, invitedByUser);
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
                async invitedByUser =>
                {
                    var user = new UserWeb2(username, invitedByUser, loginInfo);
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
                async invitedByUser =>
                {
                    var user = new UserWeb3(etherAddress, username, invitedByUser);
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

            var queryIsObjectId = ObjectId.TryParse(query, out var parsedObjectId);
            var queryAsObjectId = queryIsObjectId ? parsedObjectId.ToString() : null;

            var queryAsEtherAddress = query.IsValidEthereumAddressHexFormat() ?
                query.ConvertToEthereumChecksumAddress() : "";

            var queryAsEmail = EmailHelper.IsValidEmail(query) ?
                EmailHelper.NormalizeEmail(query) : "";

            var queryAsUsername = UsernameHelper.NormalizeUsername(query);

            var paginatedUsers = await ssoDbContext.Users.QueryPaginatedElementsAsync(elements =>
                elements.Where(filterPredicate)
                        .Where(u => u.Id == queryAsObjectId ||
                                    u.EtherAddress == queryAsEtherAddress ||
                                    u.NormalizedEmail == queryAsEmail ||
                                    u.NormalizedUsername.Contains(queryAsUsername)),
                orderKeySelector,
                page,
                take);

            return paginatedUsers;
        }

        // Helpers.
        private async Task<(IEnumerable<(string key, string msg)> errors, TUser? user)> RegisterUserHelperAsync<TUser>(
            string username,
            string? invitationCode,
            Func<UserBase?, Task<(TUser, IdentityResult)>> registerUserAsync)
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
            var (user, creationResult) = await registerUserAsync(invitation?.Emitter);

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
