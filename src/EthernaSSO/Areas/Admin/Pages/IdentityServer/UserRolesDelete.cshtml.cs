using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserRolesDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserRolesDeleteModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string RoleId { get; private set; } = default!;
        public string RoleName { get; private set; } = default!;
        public string UserId { get; private set; } = default!;
        public string Username { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string roleId, string userId)
        {
            if (roleId is null)
                throw new ArgumentNullException(nameof(roleId));
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            RoleId = roleId;
            var role = await context.Roles.FindOneAsync(roleId);
            RoleName = role.Name;

            UserId = userId;
            var user = await context.Users.FindOneAsync(userId);
            Username = user.Username;
        }

        public async Task<IActionResult> OnPostDeleteAsync(string roleId, string userId)
        {
            if (roleId is null)
                throw new ArgumentNullException(nameof(roleId));
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            var role = await context.Roles.FindOneAsync(roleId);
            var user = await context.Users.FindOneAsync(userId);
            user.RemoveRole(role.Name);
            await context.SaveChangesAsync();

            return RedirectToPage("UserRoles", new { id = userId });
        }
    }
}
