using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Options;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.SystemStore
{
    public class PersistedGrantRepository : IPersistedGrantStore
    {
        // Consts.
        public const string KeyIndexName = "keys_unique";

        // Fields.
        private readonly IMongoCollection<PersistedGrant> collection;

        // Constructor.
        public PersistedGrantRepository(DbContextOptions options, string name)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            // Register class map. (see: https://etherna.atlassian.net/browse/ESSO-140)
            BsonClassMap.RegisterClassMap<PersistedGrant>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(x => x.Key);
            });

            // Register discriminator convention. Default from MongODM doesn't work without DbContext.
            BsonSerializer.RegisterDiscriminatorConvention(typeof(PersistedGrant),
                StandardDiscriminatorConvention.Hierarchical);

            // Initialize MongoDB driver.
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DbName);
            collection = database.GetCollection<PersistedGrant>(name);
        }

        // Methods.
        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            if (filter is null)
                throw new ArgumentNullException(nameof(filter));
            filter.Validate();

            var cursor = await collection.FindAsync(BuildMongoFilterHelper(filter));
            return await cursor.ToListAsync();
        }

        public Task<PersistedGrant> GetAsync(string key) =>
            collection.AsQueryable()
                      .SingleOrDefaultAsync(x => x.Key == key);

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            if (filter is null)
                throw new ArgumentNullException(nameof(filter));
            filter.Validate();

            return collection.DeleteManyAsync(BuildMongoFilterHelper(filter));
        }

        public Task RemoveAsync(string key) =>
            collection.DeleteOneAsync(x => x.Key == key);

        public Task StoreAsync(PersistedGrant grant) =>
            collection.ReplaceOneAsync(x => x.Key == grant.Key, grant, new ReplaceOptions { IsUpsert = true });

        // Helpers.
        private FilterDefinition<PersistedGrant> BuildMongoFilterHelper(PersistedGrantFilter sourceFilter)
        {
            var fieldFilters = new List<FilterDefinition<PersistedGrant>>();

            if (!string.IsNullOrWhiteSpace(sourceFilter.ClientId))
                fieldFilters.Add(Builders<PersistedGrant>.Filter.Eq(x => x.ClientId, sourceFilter.ClientId));

            if (!string.IsNullOrWhiteSpace(sourceFilter.SessionId))
                fieldFilters.Add(Builders<PersistedGrant>.Filter.Eq(x => x.SessionId, sourceFilter.SessionId));

            if (!string.IsNullOrWhiteSpace(sourceFilter.SubjectId))
                fieldFilters.Add(Builders<PersistedGrant>.Filter.Eq(x => x.SubjectId, sourceFilter.SubjectId));

            if (!string.IsNullOrWhiteSpace(sourceFilter.Type))
                fieldFilters.Add(Builders<PersistedGrant>.Filter.Eq(x => x.Type, sourceFilter.Type));

            return Builders<PersistedGrant>.Filter.And(fieldFilters);
        }
    }
}
