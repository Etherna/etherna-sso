using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.SSOServer.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = Role.AdministratorName)]
    public class AdminModel : PageModel
    {
    }
}
