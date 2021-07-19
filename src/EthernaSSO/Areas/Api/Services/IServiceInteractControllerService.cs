using Etherna.SSOServer.Areas.Api.DtoModels;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Services
{
    public interface IServiceInteractControllerService
    {
        Task<UserContactInfoDto> GetUserContactInfoAsync(string etherAddress);
    }
}