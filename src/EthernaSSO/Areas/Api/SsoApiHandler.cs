// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Areas.Api.DtoModels;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Services.Domain;
using Etherna.SwarmSdk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api
{
    internal sealed class SsoApiHandler(
        ISsoDbContext context,
        IHttpContextAccessor httpContextAccessor,
        UserManager<UserBase> userManager,
        IUserService userService)
        : ISsoApiHandler
    {
        // Methods.
        public Task<IResult> GetCurrentUserPrivateInfoAsync() =>
            ExceptionHandler.RunAsync(async () =>
            {
                var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);
                if (user is null)
                    throw new InvalidOperationException();
                return Results.Json(new PrivateUserDto(user));
            });

        public Task<IResult> GetUserByEtherAddressAsync(EthAddress etherAddress) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var user = await userService.FindUserByAddressAsync(etherAddress);
                return Results.Json(new UserDto(user));
            });

        public Task<IResult> GetUserByUsernameAsync(string username) =>
            ExceptionHandler.RunAsync(async () =>
            {
                username = UsernameHelper.NormalizeUsername(username);
                var user = await context.Users.FindOneAsync(u => u.NormalizedUsername == username);
                return Results.Json(new UserDto(user));
            });

        public Task<IResult> GetUserContactInfoAsync(EthAddress etherAddress) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var user = await userService.FindUserByAddressAsync(etherAddress);
                return Results.Json(new UserContactInfoDto(user));
            });

        public Task<IResult> IsEmailRegisteredAsync(string email) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var result = await context.Users.QueryElementsAsync(
                    users => users.AnyAsync(u => u.NormalizedEmail == EmailHelper.NormalizeEmail(email)));
                return Results.Json(result);
            });
    }
}
