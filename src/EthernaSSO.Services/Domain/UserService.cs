﻿using Etherna.MongODM.Core.Repositories;
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
            string? invitationCode,
            Func<UserBase?, Task<(TUser, IdentityResult)>> registerUserAsync)
            where TUser : UserBase
        {
            // Verify for unique username.
            if (await userManager.FindByNameAsync(username) is not null) //if duplicate username
                return (new[] { (DuplicateUsernameErrorKey, "Username already registered.") }, null);

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
