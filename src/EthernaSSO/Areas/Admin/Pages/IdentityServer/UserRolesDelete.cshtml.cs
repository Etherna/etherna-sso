using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserRolesDeleteModel : PageModel
    {
        public string RoleId { get; private set; } = default!;
        public string UserId { get; private set; } = default!;

        public void OnGet(string roleId, string userId)
        {
            if (roleId is null)
                throw new ArgumentNullException(nameof(roleId));
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            RoleId = roleId;
            UserId = userId;
        }
    }
}
