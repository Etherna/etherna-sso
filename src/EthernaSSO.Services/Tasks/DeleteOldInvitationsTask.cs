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
