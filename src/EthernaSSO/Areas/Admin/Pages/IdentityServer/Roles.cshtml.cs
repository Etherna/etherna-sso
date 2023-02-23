//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
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
            public RoleDto(Role role)
            {
                if (role is null)
                    throw new ArgumentNullException(nameof(role));

                Id = role.Id;
                Name = role.Name;
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
        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
        public string? Query { get; private set; }
        public List<RoleDto> Roles { get; } = new();

        // Methods.
        public async Task OnGetAsync(int? p, string? q)
        {
            CurrentPage = p ?? 0;
            Query = q ?? "";

            var paginatedRoles = await context.Roles.QueryPaginatedElementsAsync(elements =>
                elements.Where(r => r.NormalizedName.Contains(Query.ToUpperInvariant())),
                r => r.NormalizedName,
                CurrentPage,
                PageSize);

            MaxPage = paginatedRoles.MaxPage;

            Roles.AddRange(paginatedRoles.Elements.Select(r => new RoleDto(r)));
        }
    }
}
