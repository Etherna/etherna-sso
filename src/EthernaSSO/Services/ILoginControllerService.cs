using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.ViewModels;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services
{
    public interface ILoginControllerService
    {
        /// <summary>
        /// Web2 login with email/username and password
        /// </summary>
        /// <param name="emailOrUsername">User's email or username</param>
        /// <param name="password">User's password</param>
        /// <param name="rememberMe">Remember user with cookie</param>
        /// <returns>The logged in user</returns>
        Task<User?> LoginWithPasswordAsync(string emailOrUsername, string password, bool rememberMe);

        /// <summary>
        /// Web3 login with wallet
        /// </summary>
        /// <param name="address">User's public address</param>
        /// <param name="signature">User's signed message</param>
        /// <returns>The logged in user</returns>
        Task<User> LoginWithWallet(string address, string signature);

        /// <summary>
        /// Logout current user
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Web2 register with email/username and password
        /// </summary>
        /// <param name="registerPswdViewModel">Registration form view model</param>
        /// <returns>The logged in user</returns>
        Task<User?> RegisterWithPasswordAsync(RegisterPswdViewModel registerPswdViewModel);

        /// <summary>
        /// Start a request of password reset for a managed account with email
        /// </summary>
        /// <param name="email">Requiring user's email</param>
        Task RequestPasswordRecoveryAsync(string email);
    }
}
