using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class UserProvidersModel : PageModel
    {
        // Models.
        public class ProviderDto
        {
            public ProviderDto(UserLoginInfo externalLogin)
            {
                if (externalLogin is null)
                    throw new ArgumentNullException(nameof(externalLogin));

                LoginProvider = externalLogin.LoginProvider;
                ProviderDisplayName = externalLogin.ProviderDisplayName;
                ProviderKey = externalLogin.ProviderKey;
            }

            public string LoginProvider { get; }
            public string ProviderDisplayName { get; }
            public string ProviderKey { get; }
        }

        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public UserProvidersModel(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public string Id { get; private set; } = default!;
        public IEnumerable<ProviderDto> Providers { get; private set; } = Array.Empty<ProviderDto>();
        public string Username { get; private set; } = default!;

        // Methods.
        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            Id = id;
            var user = await context.Users.FindOneAsync(id);

            if (user is not UserWeb2 userWeb2)
                return RedirectToPage("User", new { id });

            Providers = userWeb2.Logins.Select(l => new ProviderDto(l));
            Username = userWeb2.Username;

            return Page();
        }
    }
}
