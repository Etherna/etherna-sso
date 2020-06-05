using Etherna.MongODM;
using Etherna.MongODM.Repositories;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Domain
{
    public interface ISsoDbContext : IDbContext
    {
        ICollectionRepository<User, string> Users { get; }
        ICollectionRepository<Web3LoginToken, string> Web3LoginTokens { get; }
    }
}
