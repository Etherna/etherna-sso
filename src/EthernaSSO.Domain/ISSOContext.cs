using Digicando.MongODM.Repositories;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Domain
{
    public interface ISSOContext
    {
        ICollectionRepository<ActivityLog, string> ActivityLogs { get; }
        ICollectionRepository<User, string> Users { get; }
    }
}
