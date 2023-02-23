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
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult<string?>(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Id);
        }

        public Task<string?> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult<string?>(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string? normalizedName, CancellationToken cancellationToken)
        {
            //don't perform any action, because name normalization is already performed by domain
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(Role role, string? roleName, CancellationToken cancellationToken)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));
            if (roleName is null)
                throw new ArgumentNullException(nameof(roleName));

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
