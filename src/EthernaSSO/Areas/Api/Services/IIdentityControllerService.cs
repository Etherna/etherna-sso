using Etherna.SSOServer.Areas.Api.DtoModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Services
{
    public interface IIdentityControllerService
    {
        Task<PrivateUserDto> GetPrivateUserInfoByClaimsAsync(ClaimsPrincipal userClaims);

        Task<UserDto> GetUserByEtherAddressAsync(string etherAddress);

        Task<UserDto> GetUserByUsernameAsync(string username);
    }
}