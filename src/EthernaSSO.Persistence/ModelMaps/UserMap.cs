using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<User>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set members to ignore if null.
                    cm.GetMemberMap(u => u.Email).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.EmailConfirmed).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.EtherLoginAddress).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.EtherManagedPrivateKey).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.LockoutEnabled).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.LockoutEnd).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.Logins).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.NormalizedEmail).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.NormalizedUsername).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.PasswordHash).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.Username).SetIgnoreIfNull(true);

                    // Force serialization of readonly props.
                    cm.MapProperty(u => u.EtherAddress);
                });
        }
    }
}
