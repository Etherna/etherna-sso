using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account
{
    public class Web3LoginModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructor.
        public Web3LoginModel(ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Methods.
        public IActionResult OnGet()
            => RedirectToPage("./Login");

        public async Task<string> OnGetRetriveAuthCodeAsync(string etherAddress)
        {
            var token = await ssoDbContext.Web3LoginTokens.TryFindOneAsync(t => t.EtherAddress == etherAddress);
            if (token is null)
            {
                token = new Web3LoginToken(etherAddress);
                await ssoDbContext.Web3LoginTokens.CreateAsync(token);
            }

            return token.Code;
        }
    }
}
