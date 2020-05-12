using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Etherna.SSOServer.Services.Settings;
using Etherna.SSOServer.Services.Utilities;
using Etherna.SSOServer.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace Etherna.SSOServer.Services
{
    class LoginControllerService : ILoginControllerService
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;
        private readonly IEmailService emailService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly MVCSettings mvcSettings;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        // Constructors.
        public LoginControllerService(
            ISsoDbContext ssoDbContext,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<MVCSettings> mvcSettings,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this.ssoDbContext = ssoDbContext;
            this.emailService = emailService;
            this.httpContextAccessor = httpContextAccessor;
            this.mvcSettings = mvcSettings.Value;
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

        public Task<User> LoginWithWallet(string address, string signature)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync() => signInManager.SignOutAsync();

        public async Task<User?> RegisterWithPasswordAsync(RegisterPswdViewModel registerPswdViewModel)
        {
            // Generate new wallet.
            var address = "";
            var privateKey = "";

            // Encrypt private key.

            // Init entity.
            var user = new User(
                new EtherAccount(address, privateKey, EtherAccount.EncryptionState.ServerEncrypted),
                email: registerPswdViewModel.Email,
                username: registerPswdViewModel.Username);

            // Register user.
            var result = await userManager.CreateAsync(user, registerPswdViewModel.Password);
            
            if (result.Succeeded) return user;
            else return null;
        }

        public async Task RequestPasswordRecoveryAsync(string email)
        {
            // Get data.
            var user = await userManager.FindByEmailAsync(email);

            // Send an email with recover link.
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = new UriTemplate(
                $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}" + "{/controller}{/action}{?email,code}")
                .AddParameters(new
                {
                    controller = mvcSettings.ResetPasswordController,
                    action = mvcSettings.ResetPasswordAction,
                    email,
                    code
                }).Resolve();

            await emailService.SendEmailAsync(email, "Reset your password",
                $"Please reset yout password by clicking this link: <a href=\"{callbackUrl}\">reset password</a>");
        }
    }
}
