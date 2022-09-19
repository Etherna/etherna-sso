using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core;
using Etherna.SSOServer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Persistence.ModelMaps.Sso
{
    internal class AlphaPassRequestMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<AlphaPassRequest>("cdfb69bd-b70c-4736-9210-737b675333bc");
        }
    }
}
