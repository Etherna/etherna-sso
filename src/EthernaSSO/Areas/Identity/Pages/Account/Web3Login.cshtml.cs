using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Nethereum.Signer;
using Nethereum.Util;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    public class Web3LoginModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [EmailAddress]
            [Display(Name = "Email (optional)")]
            public string? Email { get; set; }
        }

        // Fields.
        private readonly IClientStore clientStore;
        private readonly IEventService eventService;
        private readonly IIdentityServerInteractionService idServerInteractionService;
        private readonly ILogger<ExternalLoginModel> logger;
        private readonly SignInManager<User> signInManager;
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<User> userManager;

        // Constructor.
        public Web3LoginModel(
            IClientStore clientStore,
            IEventService eventService,
            IIdentityServerInteractionService idServerInteractionService,
            ILogger<ExternalLoginModel> logger,
            SignInManager<User> signInManager,
            ISsoDbContext ssoDbContext,
            UserManager<User> userManager)
        {
            this.clientStore = clientStore;
            this.eventService = eventService;
            this.idServerInteractionService = idServerInteractionService;
            this.logger = logger;
            this.signInManager = signInManager;
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Properties.
        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public bool DuplicateEmail { get; set; }
        public string? EtherAddress { get; private set; }
        public string? ReturnUrl { get; set; }
        public string? Signature { get; private set; }

        // Methods.
        public IActionResult OnGet()
            => RedirectToPage("./Login");

        public async Task<IActionResult> OnGetRetriveAuthMessageAsync(string etherAddress)
        {
            var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            if (token is null)
            {
                token = new Web3LoginToken(etherAddress);
                await ssoDbContext.Web3LoginTokens.CreateAsync(token);
            }

            return new JsonResult(ComposeAuthMessage(token.Code));
        }

        public async Task<IActionResult> OnGetConfirmSignature(string etherAddress, string signature, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Verify signature.
            //get token
            var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            if (token is null)
            {
                ErrorMessage = $"Web3 authentication code for {etherAddress} address not found";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            //check signature
            var verifiedSignature = VerifySignature(token.Code, etherAddress, signature);

            if (!verifiedSignature)
            {
                ErrorMessage = $"Invalid signature for web3 authentication";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in user with web3 if already has an account.
            var user = await ssoDbContext.Users.TryFindOneAsync(u => u.EtherLoginAddress == etherAddress);
            if (user != null)
            {
                // Check if user is locked out.
                if (await userManager.IsLockedOutAsync(user))
                    return RedirectToPage("./Lockout");

                // Sign in.
                await signInManager.SignInAsync(user, true);

                // Delete used token.
                await ssoDbContext.Web3LoginTokens.DeleteAsync(token);

                // Check if external login is in the context of an OIDC request.
                var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                await eventService.RaiseAsync(new UserLoginSuccessEvent("web3", etherAddress, etherAddress, "web3", true, context?.Client?.ClientId));
                logger.LogInformation($"{etherAddress} logged in with web3.");

                if (context?.Client != null)
                {
                    if (await clientStore.IsPkceClientAsync(context.Client.ClientId))
                    {
                        // If the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("/Redirect", returnUrl);
                    }
                }

                return Redirect(returnUrl);
            }

            // If user does not have an account, then ask him to create one.
            else
            {
                ReturnUrl = returnUrl;
                EtherAddress = etherAddress;
                Signature = signature;

                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string etherAddress, string signature, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ReturnUrl = returnUrl;
            EtherAddress = etherAddress;
            Signature = signature;

            // Verify signature.
            //get token
            var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            if (token is null)
            {
                ErrorMessage = $"Web3 authentication code for {etherAddress} address not found";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            //check signature
            var verifiedSignature = VerifySignature(token.Code, etherAddress, signature);

            if (!verifiedSignature)
            {
                ErrorMessage = $"Invalid signature for web3 authentication";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Skip registration if invalid.
            if (ModelState.IsValid)
            {
                // Check for duplicate email.
                if (Input.Email != null)
                {
                    var emailFromUser = await userManager.FindByEmailAsync(Input.Email);
                    if (emailFromUser != null) //if duplicate email
                    {
                        ModelState.AddModelError(string.Empty, "Email already registered.");
                        DuplicateEmail = true;

                        return Page();
                    }
                }

                // Create user.
                var user = Domain.Models.User.CreateManagedWithEtherLoginAddress(etherAddress, Input.Email);

                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // Check if external login is in the context of an OIDC request.
                    var context = await idServerInteractionService.GetAuthorizationContextAsync(returnUrl);

                    await eventService.RaiseAsync(new UserLoginSuccessEvent("web3", etherAddress, etherAddress, "web3", true, context?.Client?.ClientId));
                    logger.LogInformation($"User created an account using web3 address.");

                    if (context?.Client != null)
                    {
                        if (await clientStore.IsPkceClientAsync(context.Client.ClientId))
                        {
                            // If the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.LoadingPage("/Redirect", returnUrl);
                        }
                    }

                    return Redirect(returnUrl);
                }

                // Report errors.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Show page again if there was errors.
            return Page();
        }

        // Helpers.
        private static string ComposeAuthMessage(string code) =>
            $"Sign this message for authenticate with Etherna! Code: {code}";

        private static bool VerifySignature(string authCode, string etherAccount, string signature)
        {
            var message = ComposeAuthMessage(authCode);

            var signer = new EthereumMessageSigner();
            var recAddress = signer.EncodeUTF8AndEcRecover(message, signature);

            return recAddress.IsTheSameAddress(etherAccount);
        }
    }
}
