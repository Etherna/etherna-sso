using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserClaimsDeleteModel : PageModel
    {
        public string ClaimType { get; private set; } = default!;
        public string ClaimValue { get; private set; } = default!;
        public string UserId { get; private set; } = default!;

        public void OnGet(string claimType, string claimValue, string userId)
        {
            if (claimType is null)
                throw new ArgumentNullException(nameof(claimType));
            if (claimValue is null)
                throw new ArgumentNullException(nameof(claimValue));
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            ClaimType = claimType;
            ClaimValue = claimValue;
            UserId = userId;
        }
    }
}
