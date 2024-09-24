// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.Identity
{
    public sealed class RoleStore :
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

        public Task<Role?> FindByIdAsync(string roleId, CancellationToken cancellationToken) =>
            //using try for avoid exception throwing inside Identity's userManager
            context.Roles.TryFindOneAsync(roleId, cancellationToken: cancellationToken)!;

        public async Task<Role?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) =>
            await context.Roles.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(u => u.NormalizedName == normalizedRoleName));

        public Task<string?> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(role, nameof(role));

            return Task.FromResult<string?>(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(role, nameof(role));

            return Task.FromResult(role.Id);
        }

        public Task<string?> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(role, nameof(role));

            return Task.FromResult<string?>(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string? normalizedName, CancellationToken cancellationToken)
        {
            //don't perform any action, because name normalization is already performed by domain
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(Role role, string? roleName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

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
