using Etherna.MongoDB.Driver;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Tasks
{
    public sealed class Web3LoginTokensCleanTask : IWeb3LoginTokensCleanTask
    {
        // Consts.
        public const string TaskId = "web3LoginTokensCleanTask";

        // Fields.
        private readonly ISsoDbContext ssoDbContext;

        // Constructor.
        public Web3LoginTokensCleanTask(
            ISsoDbContext ssoDbContext)
        {
            this.ssoDbContext = ssoDbContext;
        }

        // Methods.
        public Task RunAsync() =>
            ssoDbContext.Web3LoginTokens.AccessToCollectionAsync(collection =>
                collection.DeleteManyAsync(
                    Builders<Web3LoginToken>.Filter.Where(t => t.CreationDateTime < DateTime.UtcNow - TimeSpan.FromDays(1))));
    }
}
