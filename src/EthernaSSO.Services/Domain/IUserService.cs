using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    public interface IUserService
    {
        public Task<UserBase> FindUserByAddressAsync(string etherAddress);

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            string password,
            string? email,
            string? invitationCode);

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb2? user)> RegisterWeb2UserAsync(
            string username,
            UserLoginInfo loginInfo,
            string? email,
            string? invitationCode);

        public Task<(IEnumerable<(string key, string msg)> errors, UserWeb3? user)> RegisterWeb3UserAsync(
            string username,
            string etherAddress,
            string? email,
            string? invitationCode);
    }
}