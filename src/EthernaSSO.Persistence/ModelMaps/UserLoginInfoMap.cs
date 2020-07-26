﻿using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models.UserAgg;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    class UserLoginInfoMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<UserLoginInfo>("0.1.0");
        }
    }
}