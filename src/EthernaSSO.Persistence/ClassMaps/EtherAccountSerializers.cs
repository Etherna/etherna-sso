using Digicando.MongODM;
using Digicando.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models.UserAgg;

namespace Etherna.SSOServer.Persistence.ClassMaps
{
    class EtherAccountSerializers : IModelSerializerCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<EtherAccount>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set creator.
                    cm.SetCreator(() => dbContext.ProxyGenerator.CreateInstance<EtherAccount>(dbContext));

                    // Set members to ignore if default.
                    cm.GetMemberMap(a => a.PrivateKeyEncryption).SetDefaultValue(EtherAccount.EncryptionState.Plain)
                                                                .SetIgnoreIfDefault(true);

                    // Set members to ignore if null.
                    cm.GetMemberMap(u => u.PrivateKey).SetIgnoreIfNull(true);
                });
        }
    }
}
