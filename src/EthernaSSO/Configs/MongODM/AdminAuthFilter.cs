using Etherna.MongODM.AspNetCore.UI.Auth.Filters;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.MongODM
{
    public class AdminAuthFilter : IDashboardAuthFilter
    {
        public Task<bool> AuthorizeAsync(HttpContext? context)
        {
            return Task.FromResult(context?.User?.Identity?.IsAuthenticated ?? false);
        }
    }
}
