using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserDeleteModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string Id { get; private set; } = default!;
        public string Username { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            Id = id;
            var user = await context.Users.FindOneAsync(id);
            Username = user.Username;
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await context.Users.DeleteAsync(id);
            return RedirectToPage("Users");
        }
    }
}
