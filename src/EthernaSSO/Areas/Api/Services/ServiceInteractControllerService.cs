using Etherna.SSOServer.Areas.Api.DtoModels;
using Etherna.SSOServer.Services.Domain;
using Nethereum.Util;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Api.Services
{
    public class ServiceInteractControllerService : IServiceInteractControllerService
    {
        // Fields.
        private readonly IUserService userService;

        // Constructor.
        public ServiceInteractControllerService(IUserService userService)
        {
            this.userService = userService;
        }

        // Methods.
        public async Task<UserContactInfoDto> GetUserContactInfoAsync(string etherAddress)
        {
            if (!etherAddress.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("Invalid address", nameof(etherAddress));

            var user = await userService.FindUserByAddressAsync(etherAddress);
            return new UserContactInfoDto(user);
        }
    }
}
