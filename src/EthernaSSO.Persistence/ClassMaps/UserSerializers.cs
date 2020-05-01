﻿using Digicando.MongODM;
using Digicando.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ClassMaps
{
    class UserSerializers : IModelSerializerCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<User>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set creator.
                    cm.SetCreator(() => dbContext.ProxyGenerator.CreateInstance<User>(dbContext));

                    // Set Id.
                    //***

                    // Set members to ignore if null.
                    cm.GetMemberMap(u => u.Email).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.EmailConfirmed).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.EtherAccount).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.LockoutEnabled).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.LockoutEnd).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.LoginAccount).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.Logins).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.NormalizedEmail).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.NormalizedUsername).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.PasswordHash).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.Username).SetIgnoreIfNull(true);
                });
        }
    }
}
