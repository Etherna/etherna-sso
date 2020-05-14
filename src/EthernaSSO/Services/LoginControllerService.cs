using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services
{
    class LoginControllerService
    {
        // Fields.
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        // Constructors.
        public LoginControllerService(
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        // Methods.
        public async Task<User?> LoginWithPasswordAsync(string emailOrUsername, string password, bool rememberMe)
        {
            var user = emailOrUsername.Contains('@', StringComparison.InvariantCulture) ? //if is email
                await userManager.FindByEmailAsync(emailOrUsername) :
                await userManager.FindByNameAsync(emailOrUsername);

            if (user != null &&
                await userManager.CheckPasswordAsync(user, password))
            {
                await signInManager.SignInAsync(user, rememberMe);
                return user;
            }
            return null;
        }
    }
}
