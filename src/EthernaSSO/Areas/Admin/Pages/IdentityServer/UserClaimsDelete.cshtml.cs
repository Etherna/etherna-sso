using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserClaimsDeleteModel : PageModel
    {
        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserClaimsDeleteModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string ClaimType { get; private set; } = default!;
        public string ClaimValue { get; private set; } = default!;
        public string UserId { get; private set; } = default!;
        public string Username { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string claimType, string claimValue, string userId)
        {
            if (claimType is null)
                throw new ArgumentNullException(nameof(claimType));
            if (claimValue is null)
                throw new ArgumentNullException(nameof(claimValue));
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            var user = await context.Users.FindOneAsync(userId);

            ClaimType = claimType;
            ClaimValue = claimValue;
            UserId = userId;
            Username = user.Username;
        }

        public async Task<IActionResult> OnPostAsync(string claimType, string claimValue, string userId)
        {
            if (claimType is null)
                throw new ArgumentNullException(nameof(claimType));
            if (claimValue is null)
                throw new ArgumentNullException(nameof(claimValue));
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            var user = await context.Users.FindOneAsync(userId);
            user.RemoveClaim(claimType, claimValue);
            await context.SaveChangesAsync();

            return RedirectToPage("UserClaims", new { id = userId });
        }
    }
}
