using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.Db
{
    [Authorize]
    public class MigrationModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext ssoDbContext;
        private readonly UserManager<User> userManager;

        // Constructor.
        public MigrationModel(
            ISsoDbContext ssoDbContext,
            UserManager<User> userManager)
        {
            this.ssoDbContext = ssoDbContext;
            this.userManager = userManager;
        }

        // Methods.
        public void OnGet()
        {
        }

        public async Task OnPostAsync()
        {
            var userId = userManager.GetUserId(User);
            await ssoDbContext.DbMigrationManager.StartDbContextMigrationAsync(userId);
        }
    }
}
