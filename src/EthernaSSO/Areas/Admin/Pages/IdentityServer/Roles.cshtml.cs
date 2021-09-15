using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class RolesModel : PageModel
    {
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<IRoleDto> Roles { get; } = new List<IRoleDto>();

        public void OnGet()
        {
        }
    }
}
