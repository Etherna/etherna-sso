using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models.UserAgg;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    class UserClaimMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<UserClaim>("0.2.0");
        }
    }
}
