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
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    public interface IUserService
    {
        Task<UserBase> FindUserByAddressAsync(string etherAddress);

        Task<UserSharedInfo> GetSharedUserInfo(UserBase user);

        Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            string password,
            string? invitationCode);

        Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            UserLoginInfo loginInfo,
            string? invitationCode);

        Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserByAdminAsync(
            string username,
            string password,
            string? email,
            string? etherLoginAddress,
            bool lockoutEnabled,
            DateTimeOffset? lockoutEnd,
            string? phoneNumber,
            bool twoFactorEnabled);
        
        Task<(IEnumerable<(string key, string msg)> errors, UserWeb3? user)> RegisterWeb3UserAsync(
            string username,
            string etherAddress,
            string? invitationCode);

        Task<PaginatedEnumerable<UserBase>> SearchPaginatedUsersByQueryAsync<TOrderKey>(
            string? query,
            Expression<Func<UserBase, TOrderKey>> orderKeySelector,
            int page,
            int take,
            Expression<Func<UserBase, bool>>? filterPredicate = null);

        Task UpdateLockoutStatusAsync(
            UserBase user,
            bool lockoutEnabled,
            DateTimeOffset? lockoutEnd);

        Task<UserWeb3> UpgradeToWeb3(UserWeb2 userWeb2);
    }
}