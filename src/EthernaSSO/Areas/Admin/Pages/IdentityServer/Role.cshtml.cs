using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class RoleModel : PageModel
    {
        // Model.
        public class InputModel
        {
            [Required]
            [Display(Name = "Role name")]
            public string Name { get; set; } = default!;
        }

        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public RoleModel(ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string? Id { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task OnGetAsync(string? id)
        {
            Id = id;
            Input = new InputModel();

            if (id is not null)
            {
                var role = await context.Roles.FindOneAsync(id);
                Input.Name = role.Name;
            }
        }

        public async Task<IActionResult> OnPostSaveAsync(string? id)
        {
            Id = id;

            if (!ModelState.IsValid)
                return Page();

            Role role;
            if (id is null) //create
            {
                role = new Role(Input.Name);
                await context.Roles.CreateAsync(role);
            }
            else //update
            {
                role = await context.Roles.FindOneAsync(id);
                role.SetName(Input.Name);
                await context.SaveChangesAsync();
            }

            return RedirectToPage(new { role.Id });
        }
    }
}
