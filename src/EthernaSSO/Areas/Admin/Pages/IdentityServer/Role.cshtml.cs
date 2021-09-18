using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class RoleModel : PageModel
    {
        public string? Id { get; private set; } = default!;

        public void OnGet(string? id)
        {
            Id = id;
        }
    }
}
