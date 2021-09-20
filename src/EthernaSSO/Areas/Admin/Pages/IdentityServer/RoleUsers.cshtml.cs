using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Areas.Admin.Pages.IdentityServer
{
    public class RoleUsersModel : PageModel
    {
        // Models.
        public class UserDto
        {
            public UserDto(
                string id,
                string? email,
                string etherAddress,
                string username)
            {
                Id = id;
                Email = email;
                EtherAddress = etherAddress;
                Username = username;
            }

            public string Id { get; }
            public string? Email { get; }
            public string EtherAddress { get; }
            public string Username { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public RoleUsersModel(ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public int CurrentPage { get; set; }
        public int MaxPage { get; set; }
        public string? Query { get; set; }
        public string RoleId { get; private set; } = default!;
        public string RoleName { get; private set; } = default!;
        public List<UserDto> Users { get; } = new();

        // Methods.
        public async Task OnGetAsync(string id, int? p, string? q)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            CurrentPage = p ?? 0;
            Query = q ?? "";
            RoleId = id;

            var role = await context.Roles.FindOneAsync(id);
            RoleName = role.Name;

            var queryIsObjectId = MongoDB.Bson.ObjectId.TryParse(Query, out var parsedObjectId);
            var queryAsObjectId = queryIsObjectId ? parsedObjectId.ToString() : null;

            var queryAsEtherAddress = Query.IsValidEthereumAddressHexFormat() ?
                Query.ConvertToEthereumChecksumAddress() : "";

            var queryAsEmail = EmailHelper.IsValidEmail(Query) ?
                EmailHelper.NormalizeEmail(Query) : "";

            var paginatedUsers = await context.Users.QueryPaginatedElementsAsync(elements =>
                elements.Where(u => u.Roles.Contains(role))
                        .Where(u => u.Username.Contains(Query) ||
                                    u.Id == queryAsObjectId ||
                                    u.EtherAddress == queryAsEtherAddress ||
                                    u.NormalizedEmail == queryAsEmail),
                u => u.Username,
                CurrentPage,
                PageSize);

            MaxPage = paginatedUsers.MaxPage;

            Users.AddRange(paginatedUsers.Elements.Select(u => new UserDto(u.Id, u.Email, u.EtherAddress, u.Username)));
        }
    }
}
