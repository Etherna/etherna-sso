using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ClassMaps
{
    class Web3LoginTokenSerializers : IModelSerializerCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<Web3LoginToken>("0.2.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set creator.
                    cm.SetCreator(() => dbContext.ProxyGenerator.CreateInstance<Web3LoginToken>(dbContext));
                });
        }
    }
}
