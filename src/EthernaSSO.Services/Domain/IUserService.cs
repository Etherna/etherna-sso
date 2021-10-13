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

        Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            string password,
            string? invitationCode);

        Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            UserLoginInfo loginInfo,
            string? invitationCode);

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
    }
}