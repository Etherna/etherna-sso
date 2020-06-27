using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace Etherna.SSOServer.Persistence.ClassMaps
{
    class ModelBaseSerializers : IModelSerializerCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.DocumentSchemaRegister.RegisterModelSchema<ModelBase>("0.1.0");

            dbContext.DocumentSchemaRegister.RegisterModelSchema<EntityModelBase<string>>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set Id representation.
                    cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId))
                                  .SetIdGenerator(new StringObjectIdGenerator());
                });
        }
    }
}
