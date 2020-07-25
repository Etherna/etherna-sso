using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    class Web3LoginTokenMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<Web3LoginToken>("0.2.0");
        }
    }
}
