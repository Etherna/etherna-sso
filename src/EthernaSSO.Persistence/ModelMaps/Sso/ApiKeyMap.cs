using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps.Sso
{
    internal sealed class ApiKeyMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<ApiKey>("4c9f5ecd-37b7-425e-8cc4-96ec97ef443b", mm =>
            {
                mm.AutoMap();

                // Set members with custom serializers.
                mm.SetMemberSerializer(k => k.Owner, UserMap.ReferenceSerializer(dbContext));
            });
        }
    }
}
