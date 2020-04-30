using Digicando.MongODM;
using Digicando.MongODM.Repositories;
using Digicando.MongODM.Serialization;
using Digicando.MongODM.Utility;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Persistence
{
    class SsoDbContext : DbContext, ISsoDbContext
    {
        public SsoDbContext(
            IDbContextDependencies dbContextDependencies,
            DbContextOptions options)
            : base(dbContextDependencies, options)
        {
        }

        public ICollectionRepository<ActivityLog, string> ActivityLogs => throw new NotImplementedException();

        public ICollectionRepository<User, string> Users => throw new NotImplementedException();

        protected override IEnumerable<IModelSerializerCollector> SerializerCollectors => throw new NotImplementedException();
    }
}
