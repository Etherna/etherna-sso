using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.SystemStore
{
    internal sealed class SigningKeyRepository : ISigningKeyStore
    {
        // Fields.
        private readonly IMongoCollection<SerializedKey> collection;

        // Constructor.
        public SigningKeyRepository(DbContextOptions options, string name)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            // Register class map. (see: https://etherna.atlassian.net/browse/ESSO-140)
            BsonClassMap.RegisterClassMap<SerializedKey>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(k => k.Id);
            });

            // Register discriminator convention. Default from MongODM doesn't work without DbContext.
            BsonSerializer.RegisterDiscriminatorConvention(typeof(SerializedKey),
                StandardDiscriminatorConvention.Hierarchical);

            // Initialize MongoDB driver.
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DbName);
            collection = database.GetCollection<SerializedKey>(name);
        }

        // Methods.
        public Task DeleteKeyAsync(string id) =>
            collection.DeleteOneAsync(Builders<SerializedKey>.Filter.Eq(sk => sk.Id, id));

        public async Task<IEnumerable<SerializedKey>> LoadKeysAsync() =>
            await collection.AsQueryable().ToListAsync();

        public Task StoreKeyAsync(SerializedKey key) =>
            collection.InsertOneAsync(key);
    }
}
