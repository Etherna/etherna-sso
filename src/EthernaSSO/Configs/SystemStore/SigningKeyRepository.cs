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
            ArgumentNullException.ThrowIfNull(options, nameof(options));

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
