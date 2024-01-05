//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Options;
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
            ArgumentNullException.ThrowIfNull(options, nameof(options));

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
            ArgumentNullException.ThrowIfNull(filter, nameof(filter));
            filter.Validate();

            var cursor = await collection.FindAsync(BuildMongoFilterHelper(filter));
            return await cursor.ToListAsync();
        }

        public async Task<PersistedGrant?> GetAsync(string key) =>
            await collection.AsQueryable()
                            .SingleOrDefaultAsync(x => x.Key == key);

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter, nameof(filter));
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
