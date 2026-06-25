// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.SystemStore
{
    internal sealed class SigningKeyRepository : ISigningKeyStore
    {
        // Fields.
        private readonly IMongoCollection<SerializedKey> collection;

        // Constructor.
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
        public SigningKeyRepository(DbContextOptions options, string name)
        {
            ArgumentNullException.ThrowIfNull(options);

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
        public Task DeleteKeyAsync(string id, CancellationToken cancellationToken = default) =>
            collection.DeleteOneAsync(Builders<SerializedKey>.Filter.Eq(sk => sk.Id, id), cancellationToken: cancellationToken);

        public async Task<IReadOnlyCollection<SerializedKey>> LoadKeysAsync(CancellationToken cancellationToken = default) =>
            await collection.AsQueryable().ToListAsync(cancellationToken: cancellationToken);

        public Task StoreKeyAsync(SerializedKey key, CancellationToken cancellationToken = default) =>
            collection.InsertOneAsync(key, cancellationToken: cancellationToken);
    }
}
