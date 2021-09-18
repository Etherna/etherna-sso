using Etherna.MongODM.Core.Extensions;
using Etherna.SSOServer.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class RolesModel : PageModel
    {
        // Models.
        public class RoleDto
        {
            public RoleDto(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public string Id { get; }
            public string Name { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public RolesModel(ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public int CurrentPage { get; set; }
        public int MaxPage { get; set; }
        public string? Query { get; set; }
        public List<RoleDto> Roles { get; } = new();

        // Methods.
        public async Task OnGetAsync(int? p, string? q)
        {
            CurrentPage = p ?? 0;
            Query = q ?? "";
            MaxPage = (await context.Roles.QueryElementsAsync(elements =>
                elements.Where(r => r.NormalizedName.Contains(Query.ToUpperInvariant()))
                        .CountAsync()) - 1) / PageSize;

            var roles = await context.Roles.QueryElementsAsync(elements =>
                elements.Where(r => r.NormalizedName.Contains(Query.ToUpperInvariant()))
                        .Paginate(r => r.NormalizedName, CurrentPage, PageSize)
                        .ToListAsync());
            Roles.AddRange(roles.Select(r => new RoleDto(r.Id, r.Name)));
        }
    }
}
