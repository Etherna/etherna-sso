﻿using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models.UserAgg;

namespace Etherna.SSOServer.Persistence.ClassMaps
{
    class UserClaimSerializers : IModelSerializerCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<UserClaim>("0.2.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set creator.
                    cm.SetCreator(() => dbContext.ProxyGenerator.CreateInstance<UserClaim>(dbContext));
                });
        }
    }
}
