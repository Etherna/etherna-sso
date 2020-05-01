using Digicando.MongODM;
using Digicando.MongODM.Repositories;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Domain
{
    public interface ISsoDbContext : IDbContext
    {
        ICollectionRepository<User, string> Users { get; }
    }
}
