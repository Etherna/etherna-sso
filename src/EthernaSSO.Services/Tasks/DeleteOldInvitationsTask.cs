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
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public sealed class DeleteOldInvitationsTask : IDeleteOldInvitationsTask
    {
        // Consts.
        public const string TaskId = "deleteOldInvitationsTask";

        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructor.
        public DeleteOldInvitationsTask(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Methods.
        public Task RunAsync() =>
            ssoDbContext.Invitations.AccessToCollectionAsync(collection =>
                collection.DeleteManyAsync(
                    Builders<Invitation>.Filter.Where(i => i.EndLife != null && i.EndLife < DateTime.UtcNow)));
    }
}
