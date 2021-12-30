using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;
using Etherna.SSOServer.Domain.Models.UserAgg;

namespace Etherna.SSOServer.Domain
{
    public interface ISharedDbContext : IDbContext
    {
        ICollectionRepository<UserSharedInfo, string> UsersInfo { get; }
    }
}
