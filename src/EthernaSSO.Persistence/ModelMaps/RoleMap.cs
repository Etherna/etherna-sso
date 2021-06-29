using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    class RoleMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegister.AddModelMapsSchema<RoleMap>("82413cc7-9f38-4ea2-a841-4d9479ab4f11");
        }
    }
}
