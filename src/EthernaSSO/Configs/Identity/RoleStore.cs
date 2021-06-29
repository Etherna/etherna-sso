using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.Identity
{
    public sealed class RoleStore :
        IQueryableRoleStore<Role>,
        IRoleStore<Role>
    {
        // Fields.
        private readonly ISsoDbContext context;

        // Constructor.
        public RoleStore(
            ISsoDbContext context)
        {
            this.context = context;
        }

        // Properties.
        public IQueryable<Role> Roles => context.Roles.Collection.AsQueryable();

        // Methods.
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External library doesn't declare exceptions")]
        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            try { await context.Roles.CreateAsync(role, cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External library doesn't declare exceptions")]
        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            try { await context.Users.DeleteAsync(role, cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }

        public void Dispose() { }

        public Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken) =>
            //using try for avoid exception throwing inside Identity's userManager
            context.Roles.TryFindOneAsync(roleId, cancellationToken: cancellationToken)!;

        public Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) =>
            context.Roles.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.NormalizedName == normalizedRoleName));

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            //don't perform any action, because name normalization is already performed by domain
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            role.SetName(roleName);
            return Task.CompletedTask;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External library doesn't declare exceptions")]
        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            try { await context.Roles.ReplaceAsync(role, cancellationToken: cancellationToken); }
            catch { return IdentityResult.Failed(); }
            return IdentityResult.Success;
        }
    }
}
