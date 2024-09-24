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
        Task DeleteAsync(UserBase user);

        Task<UserBase> FindUserByAddressAsync(string etherAddress);

        Task<UserSharedInfo> GetSharedUserInfoAsync(UserBase user);

        Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            string password,
            string? invitationCode);

        Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserByAdminAsync(
            string username,
            string password,
            string? email,
            string? etherLoginAddress,
            bool lockoutEnabled,
            DateTimeOffset? lockoutEnd,
            string? phoneNumber,
            IEnumerable<Role> roles,
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