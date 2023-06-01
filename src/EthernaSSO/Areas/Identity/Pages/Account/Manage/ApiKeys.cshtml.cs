using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class ApiKeysModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<UserBase> userManager;

        // Constructor.
        public ApiKeysModel(
            ISsoDbContext ssoDbContext,
            UserManager<UserBase> userManager)
        {
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Properties.
        public List<ApiKey> ApiKeys { get; } = new();

        // Methods.
        public async Task<IActionResult> OnGet()
        {
            var userId = userManager.GetUserId(User);
            var apiKeys = await ssoDbContext.ApiKeys.QueryElementsAsync(elements =>
                elements.Where(k => k.Owner.Id == userId)
                        .OrderBy(k => k.CreationDateTime)
                        .ToListAsync());
            ApiKeys.AddRange(apiKeys);

            return Page();
        }
    }
}
