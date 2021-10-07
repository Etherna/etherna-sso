using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class RoleDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public RoleDeleteModel(ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string Id { get; private set; } = default!;

        [Display(Name = "Role name")]
        public string Name { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            Id = id;
            var role = await context.Roles.FindOneAsync(id);
            Name = role.Name;
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            await context.Roles.DeleteAsync(id);

            return RedirectToPage("Roles");
        }
    }
}
