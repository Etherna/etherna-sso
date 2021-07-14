using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    class InvitationMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegister.AddModelMapsSchema<Invitation>("a51c7ca1-b53e-43d2-b1ab-7efb7f5e735b", mm =>
            {
                mm.AutoMap();

                // Set members with custom serializers.
                mm.SetMemberSerializer(i => i.Emitter, UserMap.ReferenceSerializer(dbContext));
            });
        }
    }
}
