﻿//   Copyright 2021-present Etherna Sagl
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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public class DeleteOldInvitationsTask : IDeleteOldInvitationsTask
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
        public async Task RunAsync()
        {
            await ssoDbContext.Invitations.Collection.DeleteManyAsync(
                Builders<Invitation>.Filter.Where(i => i.EndLife != null && i.EndLife < DateTime.Now));
        }
    }
}
